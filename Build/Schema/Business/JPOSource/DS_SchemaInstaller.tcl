
#############################################################################
# Procedure Name : installSchema
# Brief Description : This procedure is to execute mql query
#
# Input: 1.lAdminList
#           Example :
#               {ADD}
#                   {name=Admin Object Name}
#                   {property=registry name}
#                   {version=R417}
#                   {query=QUERY1} # mql query to execute in tcl mode with proper mql syntax
#                   {query=QUERY2}
#                   {query=QUERY3}
# Outout : On Failure : returns 1
#          On Success : returns 0
#############################################################################

proc installSchema {lAdminList sAdminType} {
    puts "Installing $sAdminType Schema....."
    set sReturn 0
    # List of Admin Types for which Property Registration not required
    set lAdminTypesNotRequiredPropertyRegistration [list {page}]
    foreach lAdminInfo $lAdminList {
        # Get Admin object info
        set sErr [catch {emxGetKeyValuePairs $lAdminInfo} outStr]
        if {$sErr != 0} {
            #return -code 1 "ERROR::: Reading schema list data $lAdminInfo"
            puts stdout ">ERROR::: Reading schema list data $lAdminInfo"
            set sReturn 1
            break
        }
        set lKeyValues $outStr
        # Get Action ADD / MODIFY / DELETE
        set sAction [emxGetValueFromKey $lKeyValues action]
        if {"$sAction" == ""} {
            #return -code 1 "Action \[ADD or MODIFY or DELETE\] to be done not specified"
            puts stdout ">ERROR::: Action \[ADD or MODIFY or DELETE\] to be done not specified"
            set sReturn 1
            break
        }
        # Get Name of Admin object
        set sName [emxGetValueFromKey $lKeyValues name]
        if {"$sName" == ""} {
            #return -code 1 "Name not specified"
            puts stdout ">ERROR::: Name not specified"
            set sReturn 1
            break
        }
        puts "Processing Start::: $sAction $sAdminType $sName..."
        # Get Query
        set lQuery [emxGetValueFromKey $lKeyValues query]
        if {[llength $lQuery] == 0} {
            #return -code 1 "Query not specified"
            puts stdout ">ERROR::: Query not specified"
            set sReturn 1
            break
        }

        if {"$sAction" == "ADD"} {
            # Check already exists, if Yes continue else execute query
            set sResult [mql list $sAdminType "$sName"]
            if {"$sResult" != ""} {
                puts "EXISTS::: $sName"
                continue
            }
            # If AdminType not require registry - skip property operations
            set index [lsearch $lAdminTypesNotRequiredPropertyRegistration $sAdminType]
            if {$index == -1} {
                # Get Property
                set sProperty [emxGetValueFromKey $lKeyValues property]
                if {"$sProperty" == ""} {
                    #return -code 1 "ERROR::: Property not specified"
                    puts stdout ">ERROR::: Property not specified"
                    set sReturn 1
                    break
                }
                # check registry name uniqueness
                set sResult [mql list property name $sProperty]
                if {$sResult != ""} {
                    #return -code 1 "ERROR::: Property not unique"
                    puts stdout ">ERROR::: Property not unique"
                    set sReturn 1
                    break
                }
                # Get Version
                set sVersion [emxGetValueFromKey $lKeyValues version]
                if {"$sVersion" == ""} {
                    #return -code 1 "ERROR::: Version not specified"
                    puts stdout ">ERROR::: Version not specified"
                    set sReturn 1
                    break
                }
                # Get and append Property Query
                set lQuery [addPropertyQuery $sAdminType $sName $sProperty $sVersion Framework ENOVIAEngineering $lQuery]
            }
        }

        # Execute Query
        set sErr [catch {executeMqlQueries $lQuery} outStr]
        if {$sErr != 0} {
            #return -code 1 "$outStr"
            puts stdout ">ERROR::: $outStr"
            set sReturn 1
            break
        } else {
            puts "Success::: $sAction $sAdminType $sName..."
        }
    }
    return $sReturn
}

#############################################################################
# Procedure Name : emxGetKeyValuePairs
# Brief Description : This procedure accepts a list of key and value pair
#                     in the format specified in input argument and
#                     returns list of keys and values
# Input : List of keys and values
#         Example :
#               {ADD}
#                   {name=Admin Object Name}
#                   {property=Registry Name}
#                   {version=R417}
#                   {query=QUERY1} # mql query to execute in tcl mode with proper mql syntax
#                   {query=QUERY2}
#                   {query=QUERY3}
# Outout : Returns list of Keys and Values
#          Example :
#                   {
#                    {
#                     {action} {name} {property} {version} {query} {query} {query}
#                    }
#                    {
#                     {ADD} {Admin Object Name} {registry name} {R417} {{QUERY1} {QUERY2} {QUERY3}}
#                    }
#                   }
#############################################################################

proc emxGetKeyValuePairs {lAdminInfo} {
    # Return list of keys and values
    set lKeys {}
    set lValues {}
    foreach sItem $lAdminInfo {
        set sItem [string trim "$sItem"]
        set index [string first = $sItem]
        if {$index == -1} {
            set sValue $sItem
            if {$sValue == "ADD" || $sValue == "MODIFY" || $sValue == "DELETE"} {
                set sKey "action"
            } else {
                continue
            }
            lappend lKeys "$sKey"
            lappend lValues "$sValue"
        } else {
            set sKey [string range $sItem 0 $index-1]
            set sValue [string range $sItem $index+1 end]
            if {$sKey == "query"} {
                set nIndex [lsearch $lKeys "$sKey"]
                if {$nIndex >= 0} {
                    set lPrevValues [lindex $lValues $nIndex]
                    set lValues [lreplace $lValues $nIndex $nIndex [lappend lPrevValues $sValue] ]
                } else {
                    lappend lKeys "$sKey"
                    lappend lValues [list "$sValue"]
                }
            } else {
                lappend lKeys "$sKey"
                lappend lValues "$sValue"
            }
        }
    }
    return [list $lKeys $lValues]
}

#############################################################################
# Procedure Name : emxGetValueFromKey
# Brief Description : This procedure accepts a list of key and value pair
#                     in the format specified in input argument and
#                     returns value of the specified key.
# Input : 1. List of keys and values
#         Example :
#                   {
#                    {
#                     {action} {name} {property} {version} {query} {query} {query}
#                    }
#                    {
#                     {ADD} {Admin Object Name} {registry name} {R417} {QUERY1} {QUERY2} {QUERY3}
#                    }
#                   }
#          2. Key :
#             Example : query
# Outout : Returns Value of the specified key
#          Example : For key command
#                     {
#                      {QUERY1} {QUERY2} {QUERY2} {QUERY2}
#                     }
#
#############################################################################

proc emxGetValueFromKey {lKeyValues sKey} {
    # Get key list
    set lKeys [lindex $lKeyValues 0]

    # Get value list
    set lValues [lindex $lKeyValues 1]

    # Search for the key
    set nIndex [lsearch $lKeys "$sKey"]
    if {$nIndex == -1} {
        return ""
    }

    # Get value at same index
    return [lindex $lValues $nIndex]
}

#############################################################################
# Procedure Name : addPropertyQuery
# Brief Description : This procedure generates query to update registry name
# Input : 1. Admin Type
#         2. Admin Object Original Name
#         3. Registry Name
#         4. Version
#         5. Application
#         6. installer
# Outout : Returns list of queries
#          1. Query to update Registry Name and 
#          2. Query to update Property values on Admin Type
#############################################################################

proc addPropertyQuery {sAdminType sName sProperty sVersion sApplication sInstallOrg lQuery} {
    set sCmd "mql add property $sProperty on program eServiceSchemaVariableMapping.tcl to $sAdminType \"$sName\";";
    lappend lQuery "$sCmd"
    set sDate [clock format [clock seconds] -format %m-%d-%Y]
    set sCmd "mql modify $sAdminType \"$sName\"";
    append sCmd " property application value \"$sApplication\""
    append sCmd " property version value \"$sVersion\""
    append sCmd " property installer value \"$sInstallOrg\""
    append sCmd " property \"installed date\" value \"$sDate\""
    append sCmd " property \"original name\" value \"$sName\";"
    lappend lQuery "$sCmd"
    return $lQuery
}

#############################################################################
# Procedure Name : executeMqlQueries
# Brief Description : Read query from list and executes
# Input : 1. List of MQL queries
# Outout : Returns
#          0 Success
#          1 Fail
#############################################################################

proc executeMqlQueries {lQuery} {
puts $lQuery
    foreach sCmd $lQuery {
        set sErr [catch {eval $sCmd} outStr]
        if {$sErr != 0} {
            return -code 1 "ERROR::: Executing command $sCmd, Error Message: $outStr"
        }
    }
    return -code 0 "Success"
}