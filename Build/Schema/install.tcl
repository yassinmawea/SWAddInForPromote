tcl ;
eval {
        mql set context user creator pass "";
        mql set env global eServiceAdmin "creator";
        mql set env global eServiceAdminPwd "";
        mql set env REGISTRATIONOBJECT eServiceSchemaVariableMapping.tcl;
        mql set env PROGSCHEMAINSTALLER DS_SchemaInstaller.tcl;
        set sResult [mql list prog DS_SchemaInstaller.tcl]

        if {"$sResult" == ""} {
            mql add program DS_SchemaInstaller.tcl mql file Business/JPOSource/DS_SchemaInstaller.tcl;
        } else {
            mql modify program DS_SchemaInstaller.tcl file Business/JPOSource/DS_SchemaInstaller.tcl;
        }
        #Preparation & environment setting: End - do not touch the above block

        set PresentDir [pwd]
        set E [catch {glob -directory "$PresentDir" *.brd} NumberOfBRDFiles]
    
        if {$E==0} {
            
            if {[llength $NumberOfBRDFiles]>1} {
                puts $NumberOfBRDFiles
                
                puts "More than one build release descriptors (.brd) found...\nPlease select one:"
                            
                for {set i 0} {$i<[llength $NumberOfBRDFiles]} {incr i} {
                    puts "       $i - [lindex $NumberOfBRDFiles $i]"
                }
                puts "Waiting for your input:"
                gets stdin UserInput

                if {$UserInput>=[llength $NumberOfBRDFiles]} {
                    puts "Invalid input.."
                    return
                } else {
                    set NameOfBRDFile [lindex $NumberOfBRDFiles $UserInput]
                }
                            
            } else {
                set NameOfBRDFile [lindex $NumberOfBRDFiles 0]
            }
            set E [mql set env global BRDFILENAME $NameOfBRDFile]
            
                    puts "Executing Schema ..."
                    mql run Business/TypeData.tcl;

                    #TCLs for UI Components: Start
                    
                    #TCLs for UI Components: End

                    puts "Executing Object Data...."
                    #TCLs for objects: Start
					mql run Objects/eServiceTriggerProgramParameters.tcl;
                    #TCLs for objects: End
					
					mql insert program Business/JPOSource/INV_ReleaseDerivedOutputJPO_mxJPO.java;
					mql compile program INV_ReleaseDerivedOutputJPO;
					

        } else {
            puts "Oops...\nNo build release descriptor file found...\nPlease supply a build descriptor (.brd) file and try again...!"
        }   
        
}