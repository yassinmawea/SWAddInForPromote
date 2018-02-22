Imports ENOAPILib
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Windows.Forms
Imports SolidWorks.Interop.sldworks
Imports SolidWorks.Interop.swconst

<Guid("162ba70a-21dd-4b75-a849-4d35e0dcd76b"), ClassInterface(ClassInterfaceType.None), ProgId("SWAddInForPromote.Test")>
Public Class SWAddInForPromote
    Implements IEnoAddIn

    Public Sub GetAddInInfo(ByRef poInfo As EnoAddInInfo) Implements IEnoAddIn.GetAddInInfo
        poInfo.mbsAddInName = "SWAddInForPromote"
        poInfo.mbsCompany = "MAWEA Industries"
        poInfo.mbsDescription = "Generate new PDF Derived Output upon Released"
        poInfo.mlAddInVersion = 1
        poInfo.mlRequiredVersionMajor = EnoLibVer.EnoLibVer_Major
        poInfo.mlRequiredVersionMinor = EnoLibVer.EnoLibVer_Minor
        poInfo.mlRequiredVersionBuild = 0
    End Sub

    Public Sub InsertUIItems(poUI As IEnoUI, eUIComponent As EnoUIComponent, poSelection As EnoSelection) Implements IEnoAddIn.InsertUIItems
        Throw New NotImplementedException()
    End Sub

    'First executed when user execute "Promote" command'
    Public Sub OnCmd(poCmd As IEnoCmd) Implements IEnoAddIn.OnCmd
        Dim sel As IEnoSelection
        Dim item As IEnoSelectionItem
        Dim enoFolder As IEnoFolder = Nothing
        Dim enoFile As IEnoFile
        Dim server As IEnoServer
        Dim path As String
        Dim Type As String
        Dim checkCAD As List(Of String) = New List(Of String)
        Dim listOfComponents As IEnoSelection = New EnoSelection
        Dim listOfAssembly As IEnoSelection = New EnoSelection
        Dim listOfDrawings As IEnoSelection = New EnoSelection
        Dim listAll As IEnoSelection = New EnoSelection

        Debug.Print("Im in now!")

        sel = poCmd.Selection
        server = poCmd.Server

        If (Not ContainsSWItemsToRelease(poCmd)) Then
            Exit Sub
        End If

        ' Check if list only contains document only. If it is, exit sub.
        For Each item In sel
            path = item.GetProperty(EnoSelItemProp.Enospi_Path)
            enoFile = server.GetFileFromPath(item.Path, enoFolder)
            Type = enoFile.ObjectTypeName
            Debug.Print("Object Type is " & Type)

            ' Classify list based on types for better performance.
            If String.Compare(item.GetProperty(EnoSelItemProp.Enospi_StateCurrent), "Frozen") = 0 Then
                Debug.Print(item.GetProperty(EnoSelItemProp.Enospi_Name) & " is in Frozen state")
                listAll.AddItem(item)
                Select Case True
                    Case Type.Contains("Component")
                        Debug.Print(Type & " is added to List")
                        listOfComponents.AddItem(item)
                        UpdateINVRevisionValue(server, item, "Release")
                    Case Type.Contains("Assembly")
                        Debug.Print(Type & " is added to List")
                        listOfAssembly.AddItem(item)
                        UpdateINVRevisionValue(server, item, "Release")
                    Case Type.Contains("Drawing")
                        Debug.Print(Type & " is added to List")
                        listOfDrawings.AddItem(item)
                        UpdateINVRevisionValue(server, item, "Release")
                End Select
            End If

        Next



        ' Run Main Program
        RunProgram(poCmd, listOfComponents, listOfAssembly, listOfDrawings, listAll)

    End Sub

    Async Sub RunProgram(ByVal poCmd As IEnoCmd, ByVal listOfComponents As IEnoSelection, ByVal listOfAssembly As IEnoSelection, ByVal listOfDrawings As IEnoSelection, ByVal listAll As IEnoSelection)

        Dim t1 As Thread
        Dim t2 As Thread

        Try

            t1 = New Thread(AddressOf ProgressMessage)
            t1.SetApartmentState(ApartmentState.STA)

            'Start ProgressMessage thread while also running MainProgram
            t1.Start()
            Await Task.Run(Sub() MainProgram(poCmd, listOfComponents, listOfAssembly, listOfDrawings, listAll))

            'Abort ProgressMessage upon completing MainProgram
            t1.Abort()
            t1 = Nothing

            t2 = New Thread(AddressOf CompleteMessage)
            t2.SetApartmentState(ApartmentState.STA)
            'Start CompleteMessage
            t2.Start()

        Catch ex As Exception

        End Try

    End Sub

    'Progress Bar to indicate PDF Generation progress'
    Sub ProgressMessage()
        Dim progressBar As ProgressForm

        progressBar = New ProgressForm
        Application.Run(progressBar)
        Thread.Sleep(1000)
    End Sub

    'Complete Form to indicate Program finished executed'
    Sub CompleteMessage()
        Dim completeForm As CompleteForm

        completeForm = New CompleteForm
        Application.Run(completeForm)
        Thread.Sleep(1000)
    End Sub

    'This program is initiated as a seperate thread, act as the main program.
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="poCmd"></param>
    Sub MainProgram(ByVal poCmd As IEnoCmd, ByVal listOfComponents As IEnoSelection, ByVal listOfAssembly As IEnoSelection, ByVal listOfDrawings As IEnoSelection, ByVal listAll As IEnoSelection)
        On Error Resume Next
        Dim sel As IEnoSelection
        Dim item As IEnoSelectionItem
        Dim enoFolder As IEnoFolder = Nothing
        Dim enoFile As IEnoFile
        Dim server As IEnoServer
        Dim Type As String
        Dim checkCAD As List(Of String) = New List(Of String)
        Dim path As String
        Dim p() As Process
        Dim checkinFromExplorer As Boolean
        Dim myProcess As New Process()
        Dim swApp As SldWorks
        Dim filenameFull As String
        Dim swModel As ModelDoc2
        Dim boolstatus As Boolean
        Dim iErrors As Integer
        Dim iWarnings As Integer
        Dim collection As ICollection(Of KeyValuePair(Of String, String)) = New Dictionary(Of String, String)
        Dim msg As String = ""

        Debug.Print("In Main Program")

        sel = poCmd.Selection
        server = poCmd.Server

        ' Open SW if it's not opened yet
        p = Process.GetProcessesByName("SLDWORKS")
        checkinFromExplorer = False
        Debug.Print("P Count --> " & p.Count)
        If p.Count = 0 Then
            checkinFromExplorer = True
            myProcess.StartInfo.FileName = "C:\Program Files\SOLIDWORKS Corp\SOLIDWORKS\SLDWORKS.exe"
            myProcess.Start()
            Threading.Thread.Sleep(10000)
        End If

        'Get opened SolidWorks App
        Debug.Print("Searching swApp")
        Do
            Debug.Print("In Do Loop")
            swApp = CType(System.Runtime.InteropServices.Marshal.GetActiveObject("SldWorks.Application"), SldWorks)
            'Thread.Sleep(3000)
            Debug.Print("Checking swApp")
        Loop While (swApp Is Nothing)
        swApp.UserControl = False

        ' Get all drawing/components/assembly that is not in local
        GetLatestIteration(listAll, server)

        'Iterate Components and sync
        For Each item In listOfComponents
            path = item.GetProperty(EnoSelItemProp.Enospi_Path)
            filenameFull = Dir(path)
            Debug.Print("Component Path is " & path)
            Debug.Print("Component Filename is " & filenameFull)
            If filenameFull = "" Then
                Debug.Print("Getting Latest Copy")
                filenameFull = GetLatestCopy(item, server)
            End If

            swModel = swApp.OpenDoc6(path, swDocumentTypes_e.swDocPART, swOpenDocOptions_e.swOpenDocOptions_Silent, "", iErrors, iWarnings)
            swModel = swApp.ActivateDoc3(filenameFull, False, swRebuildOnActivation_e.swDontRebuildActiveDoc, iErrors)
            Debug.Print("what model? -> " & swModel.GetPathName)
            boolstatus = swApp.RunMacro("C:\Program Files\SolidWorks Corp\SWAddInForCheckIn\Sync1.swp", "Sync11", "main")
            swApp.QuitDoc("")
        Next

        'Iterate Assemblies and sync
        For Each item In listOfAssembly
            path = item.GetProperty(EnoSelItemProp.Enospi_Path)
            filenameFull = Dir(path)
            If filenameFull = "" Then
                Debug.Print("Getting Latest Copy")
                filenameFull = GetLatestCopy(item, server)
            End If

            swModel = swApp.OpenDoc6(path, swDocumentTypes_e.swDocASSEMBLY, swOpenDocOptions_e.swOpenDocOptions_Silent, "", iErrors, iWarnings)
            swModel = swApp.ActivateDoc3(filenameFull, False, swRebuildOnActivation_e.swDontRebuildActiveDoc, iErrors)
            Debug.Print("what model? -> " & swModel.GetPathName)
            boolstatus = swApp.RunMacro("C:\Program Files\SolidWorks Corp\SWAddInForCheckIn\Sync1.swp", "Sync11", "main")
            swApp.QuitDoc("")
        Next

        'Iterate Drawings And sync
        For Each item In listOfDrawings
            path = item.GetProperty(EnoSelItemProp.Enospi_Path)
            filenameFull = Dir(path)
            If filenameFull = "" Then
                Debug.Print("Getting Latest Copy")
                filenameFull = GetLatestCopy(item, server)
            End If
            swModel = swApp.OpenDoc6(path, swDocumentTypes_e.swDocDRAWING, swOpenDocOptions_e.swOpenDocOptions_Silent, "", iErrors, iWarnings)
            swModel = swApp.ActivateDoc3(filenameFull, False, swRebuildOnActivation_e.swDontRebuildActiveDoc, iErrors)
            Debug.Print("what model? -> " & swModel.GetPathName)
            boolstatus = swApp.RunMacro("C:\Program Files\SolidWorks Corp\SWAddInForCheckIn\Sync1.swp", "Sync11", "main")

            ' Macro to generate the PDF to the temp folder
            boolstatus = swApp.RunMacro2("C:\Program Files\SolidWorks Corp\swAddInForCheckIn\PDFDXFMacro_Alt.swp", "Personal11", "main", swRunMacroOption_e.swRunMacroUnloadAfterRun, iErrors)

            ' Invoke JPO to upload PDF and DXF to server
            UploadPDFDXFtoENOVIA(server, item)

            swApp.CloseDoc(swModel.GetTitle)
        Next

        ' Close all document including unsaved documents
        'swApp.CloseAllDocuments(True)

        ' Clear Local Cache for all the parts/assembly in the selection list
        ClearLocalCache(sel, server)

        swApp.UserControl = True

        ' If SW was not opened in the first place, then  close SW.
        If checkinFromExplorer = True Then
            myProcess.Kill()
        End If

    End Sub

    ' Invoke JPO to upload PDF and DXF to server
    Sub UploadPDFDXFtoENOVIA(ByVal server As IEnoServer, ByVal item As IEnoSelectionItem)

        Dim file As IEnoFile
        Dim attribs As IEnoAttributeValues
        Dim partNo(1) As String
        Dim rev(1) As String
        Dim objType(1) As String
        Dim jpo As IEnoJPO
        Dim result As String
        Dim parser(3) As String

        Try

            file = server.GetFileFromPath(item.Path)
            Debug.Print("file name is --> " + file.Name)

            attribs = file.GetAttributes()
            partNo(0) = attribs.GetAtt("$$name$$", "@")
            rev(0) = attribs.GetAtt("$$revision$$", "@")

            parser(0) = partNo(0)
            parser(1) = Left(rev(0), InStr(rev(0), ".") - 1)
            parser(2) = rev(0)

            Debug.Print("Coming inside UploadPDFDXFtoENOVIA")
            Debug.Print("partNo --> " + partNo(0))
            Debug.Print("rev --> " + parser(1))

            jpo = server.CreateUtility(EnoObjectType.EnoObj_EnoJPO)
            result = jpo.Execute("INV_SWDerivedOutputJPO", "createConnectDerivedOutput", parser)

        Catch e As Exception
            MsgBox(Err.Description)
        End Try

        Exit Sub
    End Sub

    ' Invoke JPO to update revision value
    Sub UpdateINVRevisionValue(ByVal server As IEnoServer, ByVal item As IEnoSelectionItem, ByVal operation As String)

        Dim file As IEnoFile
        Dim attribs As IEnoAttributeValues
        Dim partNo(1) As String
        Dim rev(1) As String
        Dim objType(1) As String
        Dim jpo As IEnoJPO
        Dim result As String
        Dim parser(5) As String
        Dim objID(1) As String
        Dim ops(1) As String

        Try

            file = server.GetFileFromPath(item.Path)
            Debug.Print("file name is --> " + file.Name)

            attribs = file.GetAttributes()
            objID(0) = ""
            partNo(0) = attribs.GetAtt("$$name$$", "@")
            rev(0) = attribs.GetAtt("$$revision$$", "@")
            objType(0) = file.ObjectTypeName
            ops(0) = operation

            parser(0) = objID(0)
            parser(1) = partNo(0)
            parser(2) = Left(rev(0), InStr(rev(0), ".") - 1)
            parser(3) = objType(0)
            parser(4) = ops(0)


            Debug.Print("Coming inside updateINVRevisionValue")
            Debug.Print("partNo --> " + partNo(0))
            Debug.Print("rev --> " + parser(2))
            Debug.Print("type --> " + parser(3))

            jpo = server.CreateUtility(EnoObjectType.EnoObj_EnoJPO)
            result = jpo.Execute("INV_ReleaseDerivedOutputJPO", "updateINVRevisionValue", parser)

        Catch e As Exception
            MsgBox(Err.Description)
        End Try

        Exit Sub
    End Sub

    Private Function IsToRelease(ByVal poCmd As IEnoCmd) As Boolean
        Dim bool As Boolean = False
        Dim sel As IEnoSelection
        Dim item As IEnoSelectionItem

        sel = poCmd.Selection

        For Each item In sel
            If String.Compare(item.GetProperty(EnoSelItemProp.Enospi_StateCurrent), "Frozen") Then
                bool = True
                Exit For
            End If
        Next

        Return bool
    End Function

    Private Function ContainsSWItemsToRelease(ByVal poCmd As IEnoCmd) As Boolean
        Dim sel As IEnoSelection
        Dim bool As Boolean = False
        Dim item As IEnoSelectionItem
        Dim server As IEnoServer
        Dim enoFile As IEnoFile
        Dim enoFolder As IEnoFolder = Nothing


        sel = poCmd.Selection
        server = poCmd.Server


        For Each item In sel
            Debug.Print("--------Find Prop---------")
            enoFile = server.GetFileFromPath(item.Path, enoFolder)
            Debug.Print("Item Type is " & enoFile.ObjectTypeName & " --bool --> " & enoFile.ObjectTypeName.Contains("SW"))
            Debug.Print("Item State is " & item.GetProperty(EnoSelItemProp.Enospi_StateCurrent) & " --bool --> " & String.Compare(item.GetProperty(EnoSelItemProp.Enospi_StateCurrent), "Frozen"))
            Debug.Print("-----------------")
            If enoFile.ObjectTypeName.Contains("SW") AndAlso String.Compare(item.GetProperty(EnoSelItemProp.Enospi_StateCurrent), "Frozen") = 0 Then
                bool = True
                Exit For
            End If
        Next

        Return bool
    End Function

    Public Function GetLatestCopy(ByVal item As IEnoSelectionItem, ByVal server As IEnoServer) As String
        Dim enoFile As IEnoFile
        Dim enoFolder As IEnoFolder = Nothing
        Dim path As String
        Dim filenameFull As String

        enoFile = server.GetFileFromPath(item.Path, enoFolder)
        enoFile.GetFileCopy(Nothing, Nothing, Nothing, enoFolder.ID)

        path = item.GetProperty(EnoSelItemProp.Enospi_Path)
        filenameFull = Dir(path)

        Debug.Print("Latest Copy Filename -> " & filenameFull)
        Return filenameFull

    End Function

    Sub ClearLocalCache(ByVal selection As IEnoSelection, ByVal server As IEnoServer)
        Dim clc As IEnoClearLocalCache

        clc = server.CreateUtility(EnoObjectType.EnoObj_EnoClearLocalCache)
        clc.AddSelection(selection)
        clc.IgnoreToolboxFiles = True
        clc.Commit()

    End Sub

    Sub GetLatestIteration(ByVal selection As IEnoSelection, ByVal server As IEnoServer)
        Dim gli As IEnoBatchGet

        Debug.Print("In GetLatestIteration")
        gli = server.CreateUtility(EnoObjectType.EnoObj_EnoBatchGet)
        gli.AddSelection(selection)
        gli.Prepare(0, EnoGetFlags.EnoGet_RefreshFileListing)
        gli.Commit(0)
        Debug.Print("After Commit")

    End Sub

End Class
