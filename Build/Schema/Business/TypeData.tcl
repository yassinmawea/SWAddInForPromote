tcl;
eval {
###############################################################################
#
# Procedure:    utLoad
#
# Description:  Procedure to load other tcl utilities procedures.
#
# Parameters:   sProgram                - Tcl file to load
#
# Returns:      sOutput                 - Filtered tcl file
#               glUtLoadProgs           - List of loaded programs
#
###############################################################################

proc utLoad { sProgram } {

    global glUtLoadProgs env

    if { ! [ info exists glUtLoadProgs ] } {
        set glUtLoadProgs {}
    }

    if { [ lsearch $glUtLoadProgs $sProgram ] < 0 } {
        lappend glUtLoadProgs $sProgram
    } else {
        return ""
    }

    if { [ catch {
        set sDir "$env(TCL_LIBRARY)/mxTclDev"
        set pFile [ open "$sDir/$sProgram" r ]
        set sOutput [ read $pFile ]
        close $pFile

    } ] == 0 } { return $sOutput }

    set  sOutput [ mql print program \"$sProgram\" select code dump ]

    return $sOutput
}
# end utload
###############################################################################

    mql verbose on;
    set Item "type"
    set RegProgName [mql get env PROGSCHEMAINSTALLER]
    #Load Utility function
    eval [utLoad $RegProgName]


#-------------------------------------------------------------------------
# MOD Type : type_SWComponentInstance 
#-------------------------------------------------------------------------
set lList_Modify_type_SWComponentInstance \
              [ list {MODIFY} \
                   {version=R417} \
                   {property=type_SWComponentInstance} \
                   {name=SW Component Instance} \
                   {query=mql mod type "SW Component Instance" add attribute "INV_Revision";} \
              ]
#-------------------------------------------------------------------------
#-------------------------------------------------------------------------
# MOD Type : type_SWAssemblyInstance
#-------------------------------------------------------------------------
set lList_Modify_type_SWAssemblyInstance \
              [ list {MODIFY} \
                   {version=R417} \
                   {property=type_SWAssemblyInstance} \
                   {name=SW Assembly Instance} \
                   {query=mql mod type "SW Assembly Family" add attribute "INV_Revision";} \
              ]
#-------------------------------------------------------------------------
#-------------------------------------------------------------------------
# MOD Type : type_SWDrawing
#-------------------------------------------------------------------------
set lList_Modify_type_SWDrawing \
              [ list {MODIFY} \
                   {version=R417} \
                   {property=type_SWDrawing} \
                   {name=SW Assembly Instance} \
                   {query=mql mod type "SW Drawing" add attribute "INV_Revision";} \
              ]
#-------------------------------------------------------------------------
#----------------------------------Start----------------------------------
# Modify Type : SW Assembly Family
#-------------------------------------------------------------------------
           set lList_Modify_SWAssemblyFamily \
              [ list {MODIFY} \
                   {version=R417} \
                   {property=type_SWAssemblyFamily} \
                   {name=SW Assembly Family} \
                   {query=mql mod type "SW Assembly Family" remove trigger checkin add trigger checkin check emxTriggerManager input "TypeSWCheckinCheck" action emxTriggerManager input "TypeSWCheckinAction";} \
              ]
#----------------------------------Start----------------------------------
# Modify Type : SW Component Family
#-------------------------------------------------------------------------
           set lList_Modify_ComponentFamily \
              [ list {MODIFY} \
                   {version=R417} \
                   {property=type_SWComponentFamily} \
                   {name=SW Component Family} \
                   {query=mql mod type "SW Component Family" remove trigger checkin add trigger checkin check emxTriggerManager input "TypeSWCheckinCheck" action emxTriggerManager input "TypeSWCheckinAction";} \
              ]
#----------------------------------Start----------------------------------
           
			set E [mql set env global CURRENT_EXECUTION $Item]
			source "[mql get env BRDFILENAME]"

			set mqlret [installSchema $lCommandList $Item]
			return -code $mqlret ""
}