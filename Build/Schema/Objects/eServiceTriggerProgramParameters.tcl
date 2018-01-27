#################################################################################
#                                                                               #
# Description :                                                                 #
#                                                                               #
#################################################################################

# Call the TCL Prompt
tcl;

eval { 

#################################################################################
#                                                                               #
# Procedure:    utLoad                                                          #
#                                                                               #
# Description:  Procedure to load other tcl utilities procedures.               #
#                                                                               #
# Parameters:   sProgram                - Tcl file to load                      #
#                                                                               #
# Returns:      sOutput                 - Filtered tcl file                     #
#               glUtLoadProgs           - List of loaded programs               #
#                                                                               #
#################################################################################

proc utLoad { sProgram } {

    global glUtLoadProgs env

    if { ! [ info exists glUtLoadProgs ]} {
        set glUtLoadProgs {}
    }

    if { [ lsearch $glUtLoadProgs $sProgram ]< 0 } {
        lappend glUtLoadProgs $sProgram
    } else {
        return ""
    }

    if { [ catch {
        set sDir "$env(TCL_LIBRARY)/mxTclDev"
        set pFile [ open "$sDir/$sProgram" r ]
        set sOutput [ read $pFile ]
        close $pFile

    } ]== 0 } { return $sOutput }

    set  sOutput [ mql print program \"$sProgram\" select code dump ]

    return $sOutput
}
#end utload

#################################################################################

   mql verbose on;

   set RegProgName   [mql get env REGISTRATIONOBJECT]

   # Load Utility function
   eval [utLoad $RegProgName]


#-------------------------------------Start-------------------------------------
# Name:         TypeSWCheckinCheck
# Revision:     INV_CheckFileNameUnique
#-------------------------------------------------------------------------------
    set lList_TypeSWCheckinAction \
        [ list {Adding Trigger ...} \
               {ADD_OBJECT} \
               {type_eServiceTriggerProgramParameters} \
               {TypeSWCheckinAction} \
               {INV_UpdateINVRevisionValue} \
               {mql add businessobject \
                        "$type_eServiceTriggerProgramParameters" "TypeSWCheckinAction" "INV_UpdateINVRevisionValue" \
                        policy "$policy_eServiceTriggerProgramPolicy" \
                        vault "$vault_eServiceAdministration" \
                        "$attribute_eServiceProgramName" "INV_ReleaseDerivedOutputJPO" \
                        "$attribute_eServiceMethodName" "updateINVRevisionValue" \
                        "$attribute_eServiceSequenceNumber" "1" \
                        "$attribute_eServiceProgramArgument1" "\${OBJECTID}" \
                        "$attribute_eServiceProgramArgument2" "\${NAME}" \
						"$attribute_eServiceProgramArgument3" "\${REVISION}" \
						"$attribute_eServiceProgramArgument4" "\${TYPE}" \
						"$attribute_eServiceProgramArgument5" "Checkin" \
               } \
        ]

    set lList_ActivateTypeSWCheckinAction \
        [ list {Activating ...} \
               {CHANGE_STATE} \
               {type_eServiceTriggerProgramParameters} \
               {TypeSWCheckinAction} \
               {INV_UpdateINVRevisionValue} \
               {state_Active} \
        ]
#--------------------------------------------------------------------------


        set E [mql set env global CURRENT_EXECUTION eServiceTriggerProgramParameters]
        source "[mql get env BRDFILENAME]"

        set mqlret [eServiceInstallAdminBusObjs $lCommandList]
        return -code $mqlret ""

}

