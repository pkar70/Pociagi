﻿Imports Windows.ApplicationModel.DataTransfer
Imports Windows.Data.Json
Imports Windows.Storage
Imports Windows.UI.Core
Imports Windows.UI.Popups
Imports Windows.Web.Http
''' <summary>
''' Provides application-specific behavior to supplement the default Application class.
''' </summary>
NotInheritable Class App
    Inherits Application

#Region "Autogenerated"

    ''' <summary>
    ''' Invoked when the application is launched normally by the end user.  Other entry points
    ''' will be used when the application is launched to open a specific file, to display
    ''' search results, and so forth.
    ''' </summary>
    ''' <param name="e">Details about the launch request and process.</param>
    Protected Overrides Sub OnLaunched(e As Windows.ApplicationModel.Activation.LaunchActivatedEventArgs)
        Dim rootFrame As Frame = TryCast(Window.Current.Content, Frame)

        ' Do not repeat app initialization when the Window already has content,
        ' just ensure that the window is active

        If rootFrame Is Nothing Then
            ' Create a Frame to act as the navigation context and navigate to the first page
            rootFrame = New Frame()

            AddHandler rootFrame.NavigationFailed, AddressOf OnNavigationFailed

            ' PKAR added wedle https://stackoverflow.com/questions/39262926/uwp-hardware-back-press-work-correctly-in-mobile-but-error-with-pc
            AddHandler rootFrame.Navigated, AddressOf OnNavigatedAddBackButton
            AddHandler SystemNavigationManager.GetForCurrentView().BackRequested, AddressOf OnBackButtonPressed

            If e.PreviousExecutionState = ApplicationExecutionState.Terminated Then
                ' TODO: Load state from previously suspended application
            End If
            ' Place the frame in the current Window
            Window.Current.Content = rootFrame
        End If

        If e.PrelaunchActivated = False Then
            If rootFrame.Content Is Nothing Then
                ' When the navigation stack isn't restored navigate to the first page,
                ' configuring the new page by passing required information as a navigation
                ' parameter
                rootFrame.Navigate(GetType(MainPage), e.Arguments)
            End If

            ' Ensure the current window is active
            Window.Current.Activate()
        End If
    End Sub

    ''' <summary>
    ''' Invoked when Navigation to a certain page fails
    ''' </summary>
    ''' <param name="sender">The Frame which failed navigation</param>
    ''' <param name="e">Details about the navigation failure</param>
    Private Sub OnNavigationFailed(sender As Object, e As NavigationFailedEventArgs)
        Throw New Exception("Failed to load Page " + e.SourcePageType.FullName)
    End Sub

    ''' <summary>
    ''' Invoked when application execution is being suspended.  Application state is saved
    ''' without knowing whether the application will be terminated or resumed with the contents
    ''' of memory still intact.
    ''' </summary>
    ''' <param name="sender">The source of the suspend request.</param>
    ''' <param name="e">Details about the suspend request.</param>
    Private Sub OnSuspending(sender As Object, e As SuspendingEventArgs) Handles Me.Suspending
        Dim deferral As SuspendingDeferral = e.SuspendingOperation.GetDeferral()
        ' TODO: Save application state and stop any background activity
        deferral.Complete()
    End Sub

#End Region


#Region "moja biblioteka"

    ' PKAR added wedle https://stackoverflow.com/questions/39262926/uwp-hardware-back-press-work-correctly-in-mobile-but-error-with-pc
    Private Sub OnNavigatedAddBackButton(sender As Object, e As NavigationEventArgs)
        Dim oFrame = TryCast(sender, Frame)
        Dim oNavig = SystemNavigationManager.GetForCurrentView

        If oFrame.CanGoBack Then
            oNavig.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible
        Else
            oNavig.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed
        End If

    End Sub

    Private Sub OnBackButtonPressed(sender As Object, e As BackRequestedEventArgs)
        Try
            TryCast(Window.Current.Content, Frame).GoBack()
            e.Handled = True
        Catch ex As Exception
        End Try
    End Sub

#Region "Settings"

    Public Shared Function GetSettingsString(sName As String, Optional sDefault As String = "") As String
        Dim sTmp As String

        sTmp = sDefault

        If ApplicationData.Current.RoamingSettings.Values.ContainsKey(sName) Then
            sTmp = ApplicationData.Current.RoamingSettings.Values(sName).ToString
        End If
        If ApplicationData.Current.LocalSettings.Values.ContainsKey(sName) Then
            sTmp = ApplicationData.Current.LocalSettings.Values(sName).ToString
        End If

        Return sTmp

    End Function

    Public Shared Sub SetSettingsString(sName As String, sValue As String, Optional bRoam As Boolean = False)
        Try
            If bRoam Then ApplicationData.Current.RoamingSettings.Values(sName) = sValue
            ApplicationData.Current.LocalSettings.Values(sName) = sValue
        Catch ex As Exception

        End Try
        ' try/catch, bo zmienna ma długość limitowaną (i InternalLog jest za krótki)
    End Sub


    Public Shared Function GetSettingsInt(sName As String, Optional iDefault As Integer = 0) As Integer
        Dim sTmp As Integer

        sTmp = iDefault

        If ApplicationData.Current.RoamingSettings.Values.ContainsKey(sName) Then
            sTmp = CInt(ApplicationData.Current.RoamingSettings.Values(sName).ToString)
        End If
        If ApplicationData.Current.LocalSettings.Values.ContainsKey(sName) Then
            sTmp = CInt(ApplicationData.Current.LocalSettings.Values(sName).ToString)
        End If

        Return sTmp

    End Function

    Public Shared Sub SetSettingsInt(sName As String, sValue As Integer, Optional bRoam As Boolean = False)
        If bRoam Then ApplicationData.Current.RoamingSettings.Values(sName) = sValue.ToString
        ApplicationData.Current.LocalSettings.Values(sName) = sValue.ToString
    End Sub


    Public Shared Function GetSettingsBool(sName As String, Optional iDefault As Boolean = False) As Boolean
        Dim sTmp As Boolean

        sTmp = iDefault

        If ApplicationData.Current.RoamingSettings.Values.ContainsKey(sName) Then
            sTmp = CBool(ApplicationData.Current.RoamingSettings.Values(sName).ToString)
        End If
        If ApplicationData.Current.LocalSettings.Values.ContainsKey(sName) Then
            sTmp = CBool(ApplicationData.Current.LocalSettings.Values(sName).ToString)
        End If

        Return sTmp

    End Function

    Public Shared Sub SetSettingsBool(sName As String, sValue As Boolean, Optional bRoam As Boolean = False)
        If bRoam Then ApplicationData.Current.RoamingSettings.Values(sName) = sValue.ToString
        ApplicationData.Current.LocalSettings.Values(sName) = sValue.ToString
    End Sub
#End Region

    Public Shared Function XmlSafeString(sInput As String) As String
        If sInput Is Nothing Then Return Nothing
        Dim sTmp As String
        sTmp = sInput.Replace("&", "&amp;")
        sTmp = sTmp.Replace("<", "&lt;")
        sTmp = sTmp.Replace(">", "&gt;")
        Return sTmp
    End Function
#Region "Dialogi"

    'Public Shared Sub MakeToast(sMsg As String, Optional sMsg1 As String = "")
    '    Dim sXml = "<visual><binding template='ToastGeneric'><text>" & XmlSafeString(sMsg)
    '    If sMsg1 <> "" Then sXml = sXml & "</text><text>" & XmlSafeString(sMsg1)
    '    sXml = sXml & "</text></binding></visual>"
    '    Dim oXml = New XmlDocument
    '    oXml.LoadXml("<toast>" & sXml & "</toast>")
    '    Dim oToast = New ToastNotification(oXml)
    '    ToastNotificationManager.CreateToastNotifier().Show(oToast)
    'End Sub

    Public Shared Async Sub DialogBox(sMsg As String)
        Dim oMsg As New MessageDialog(sMsg)
        Await oMsg.ShowAsync
    End Sub
    Public Shared Async Sub DialogBoxError(iNr As Integer, sMsg As String)
        Dim sTxt = "ERROR"
        sTxt = sTxt & " (" & iNr & ")" & vbCrLf & sMsg
        Dim oMsg As New MessageDialog(sTxt)
        Await oMsg.ShowAsync
    End Sub

    Public Shared Async Function DialogBoxResYN(sMsgResId As String, Optional sYesResId As String = "resDlgYes", Optional sNoResId As String = "resDlgNo") As Task(Of Boolean)
        Dim sMsg, sYes, sNo As String

        With Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView()
            sMsg = .GetString(sMsgResId)
            sYes = .GetString(sYesResId)
            sNo = .GetString(sNoResId)
        End With

        Dim oMsg As New MessageDialog(sMsg)
        Dim oYes = New UICommand(sYes)
        Dim oNo = New UICommand(sNo)
        oMsg.Commands.Add(oYes)
        oMsg.Commands.Add(oNo)
        oMsg.DefaultCommandIndex = 1    ' default: No
        Dim oCmd = Await oMsg.ShowAsync
        If oCmd Is Nothing Then Return False
        If oCmd.Label = sYes Then Return True

        Return False

    End Function

    Public Shared Async Function DialogBoxInput(sMsgResId As String, Optional sDefaultResId As String = "", Optional sYesResId As String = "resDlgContinue", Optional sNoResId As String = "resDlgCancel") As Task(Of String)
        Dim sMsg, sYes, sNo, sDefault As String

        sDefault = ""

        With Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView()
            sMsg = .GetString(sMsgResId)
            sYes = .GetString(sYesResId)
            sNo = .GetString(sNoResId)
            If sDefaultResId <> "" Then sDefault = .GetString(sDefaultResId)
        End With

        If sMsg = "" Then sMsg = sMsgResId  ' zabezpieczenie na brak string w resource
        If sYes = "" Then sYes = sYesResId
        If sNo = "" Then sNo = sNoResId
        If sDefault = "" Then sDefault = sDefaultResId

        Dim oInputTextBox = New TextBox
        oInputTextBox.AcceptsReturn = False
        oInputTextBox.Text = sDefault
        Dim oDlg As New ContentDialog
        oDlg.Content = oInputTextBox
        oDlg.PrimaryButtonText = sYes
        oDlg.SecondaryButtonText = sNo
        oDlg.Title = sMsg

        Dim oCmd = Await oDlg.ShowAsync
        If oCmd <> ContentDialogResult.Primary Then Return ""

        Return oInputTextBox.Text

    End Function
#End Region

    Public Shared Function IsMobile() As Boolean
        Return (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily = "Windows.Mobile")
    End Function
    Public Shared Async Function GetDataFolder(sSubFolder As String) As Task(Of StorageFolder)
        Dim oFold As StorageFolder

        oFold = Windows.Storage.ApplicationData.Current.LocalFolder
        If sSubFolder = "" Then Return oFold

        Return Await oFold.CreateFolderAsync(sSubFolder, CreationCollisionOption.OpenIfExists)
    End Function

    Public Shared Async Function GetDataFile(sName As String, bCreate As Boolean, Optional sSubDir As String = "", Optional bRewrite As Boolean = False) As Task(Of StorageFile)
        Dim oFold = Await GetDataFolder(sSubDir)
        If oFold Is Nothing Then Return Nothing

        Dim oCCO As CreationCollisionOption = CreationCollisionOption.OpenIfExists
        If bRewrite Then oCCO = CreationCollisionOption.ReplaceExisting


        Dim bErr = False
        Dim oFile = Nothing
        Try
            If bCreate Then
                oFile = Await oFold.CreateFileAsync(sName, oCCO)
            Else
                oFile = Await oFold.GetFileAsync(sName)
            End If
        Catch ex As Exception
            bErr = True
        End Try
        If bErr Then
            Return Nothing
        End If

        Return oFile
    End Function

    Public Shared Function IsNetIPavailable(bMsg As Boolean) As Boolean
        If App.GetSettingsBool("offline") Then Return False

        If Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable() Then Return True
        If bMsg Then
            DialogBox("ERROR: no IP network available")
        End If
        Return False
    End Function

    Public Shared Function IsCellInet() As Boolean
        Return Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile().IsWwanConnectionProfile
    End Function

    Public Shared Sub ClipPut(sTxt As String)
        Dim oClipCont = New DataPackage
        oClipCont.RequestedOperation = DataPackageOperation.Copy
        oClipCont.SetText(sTxt)
        Clipboard.SetContent(oClipCont)
    End Sub

    Public Shared Async Function ClipGet() As Task(Of String)
        Dim oClipCont = Clipboard.GetContent
        Return Await oClipCont.GetTextAsync()
    End Function

#End Region

    Public Shared moHttp As HttpClient = New HttpClient
    Public Shared Async Function HttpPageAsync(sUrl As String, sErrMsg As String, Optional sData As String = "") As Task(Of String)
        If Not App.IsNetIPavailable(True) Then Return ""
        If sUrl = "" Then Return ""

        'If App.moHttpHand Is Nothing Then
        '    App.moHttpHand = New HttpClientHandler
        '    App.moHttpHand.UseCookies = True
        '    App.moHttpHand.AllowAutoRedirect = True
        'End If
        ' If App.moHttp Is Nothing Then App.moHttp = New HttpClient(App.moHttpHand)
        If App.moHttp Is Nothing Then App.moHttp = New HttpClient

        Dim sError = ""
        Dim oResp As HttpResponseMessage = Nothing

        Try
            If sData <> "" Then
                Dim oHttpCont = New HttpStringContent(sData, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded")
                oResp = Await App.moHttp.PostAsync(New Uri(sUrl), oHttpCont)
            Else
                oResp = Await App.moHttp.GetAsync(New Uri(sUrl))
            End If
        Catch ex As Exception
            sError = ex.Message
        End Try

        If sError <> "" Then
            App.DialogBox("error " & sError & " at " & sErrMsg & " page")
            Return ""
        End If

        If oResp.StatusCode = 303 Or oResp.StatusCode = 302 Or oResp.StatusCode = 301 Then
            ' redirect
            sUrl = oResp.Headers.Location.ToString
            'If sUrl.ToLower.Substring(0, 4) <> "http" Then
            '    sUrl = "https://sympatia.onet.pl/" & sUrl   ' potrzebne przy szukaniu
            'End If

            If sData <> "" Then
                ' Dim oHttpCont = New HttpStringContent(sData, Text.Encoding.UTF8, "application/x-www-form-urlencoded")
                Dim oHttpCont = New HttpStringContent(sData, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded")
                oResp = Await App.moHttp.PostAsync(New Uri(sUrl), oHttpCont)
            Else
                oResp = Await App.moHttp.GetAsync(New Uri(sUrl))
            End If
        End If

        If oResp.StatusCode > 290 Then
            App.DialogBox("ERROR " & oResp.StatusCode & " getting " & sErrMsg & " page")
            Return ""
        End If

        Dim sResp As String = ""
        Try
            sResp = Await oResp.Content.ReadAsStringAsync
        Catch ex As Exception
            sError = ex.Message
        End Try

        If sError <> "" Then
            App.DialogBox("error " & sError & " at ReadAsStringAsync " & sErrMsg & " page")
            Return ""
        End If

        Return sResp

    End Function

    Public Shared moPunkty As GeoPunkty = Nothing

    Public Shared Async Function InitPunkty(bForce As Boolean) As Task(Of Boolean)
        If moPunkty IsNot Nothing And Not bForce Then Return True   ' jest juz
        moPunkty = New GeoPunkty
        Return Await moPunkty.LoadAsync
    End Function

    Public Shared Sub OpenBrowser(sUri As String, bForceEdge As Boolean)
        Dim oUri As Uri = New Uri(sUri)
        If bForceEdge Then
            Dim options As Windows.System.LauncherOptions = New Windows.System.LauncherOptions()
            options.TargetApplicationPackageFamilyName = "Microsoft.MicrosoftEdge_8wekyb3d8bbwe"
            Windows.System.Launcher.LaunchUriAsync(oUri, options)
        Else
            Windows.System.Launcher.LaunchUriAsync(oUri)
        End If

    End Sub

    Public Shared Function RemoveHtmlTags(sHtml As String) As String
        Dim iInd0, iInd1 As Integer

        iInd0 = sHtml.IndexOf("<script")
        If iInd0 > 0 Then
            iInd1 = sHtml.IndexOf("</script>", iInd0)
            If iInd1 > 0 Then
                sHtml = sHtml.Remove(iInd0, iInd1 - iInd0 + 9)
            End If
        End If

        iInd0 = sHtml.IndexOf("<")
        iInd1 = sHtml.IndexOf(">")
        While iInd0 > -1
            If iInd1 > -1 Then
                sHtml = sHtml.Remove(iInd0, iInd1 - iInd0 + 1)
            Else
                sHtml = sHtml.Substring(0, iInd0)
            End If
            sHtml = sHtml.Trim

            iInd0 = sHtml.IndexOf("<")
            iInd1 = sHtml.IndexOf(">")
        End While

        sHtml = sHtml.Replace("&nbsp;", " ")
        sHtml = sHtml.Replace(vbLf, vbCrLf)
        sHtml = sHtml.Replace(vbCrLf & vbCrLf, vbCrLf)
        sHtml = sHtml.Replace(vbCrLf & vbCrLf, vbCrLf)
        sHtml = sHtml.Replace(vbCrLf & vbCrLf, vbCrLf)

        Return sHtml.Trim

    End Function

    Public Shared Async Function GetOnlineStationNameAsync(sMask As String, bOnlyNames As Boolean) As Task(Of Object)
        Dim oColl As Collection(Of GeoPunkt) = New Collection(Of GeoPunkt)

        If sMask.Length < 3 Then Return oColl

        Dim sUrl As String = "http://rozklad-pkp.pl/station/search?short=0&term=" & Uri.EscapeUriString(sMask)
        Dim sPage = Await App.HttpPageAsync(sUrl, "identyfikatory stacji")

        Dim oJson As JsonObject = Nothing
        Dim bError As Boolean = False
        Try
            oJson = JsonObject.Parse("{""stops"": " & sPage & "}")
        Catch ex As Exception
            bError = True
        End Try
        If bError Then
            DialogBox("ERROR: JSON parsing error")
            Return oColl
        End If

        Dim oJsonStops As New JsonArray

        Try
            oJsonStops = oJson.GetNamedArray("stops")
        Catch ex As Exception
            bError = True
        End Try
        If bError Then
            DialogBox("ERROR: JSON 'stops' array missing")
            Return oColl
        End If

        For Each oItem In oJsonStops
            Dim oNew As GeoPunkt = New GeoPunkt
            oNew.nazwa = System.Net.WebUtility.HtmlDecode(oItem.GetObject.GetNamedString("name"))
            oNew.idPKP = oItem.GetObject.GetNamedString("value").Substring(2, 5)
            oColl.Add(oNew)
        Next

        If bOnlyNames Then
            Return From c In oColl Select c.nazwa
        Else
            Return oColl
        End If
    End Function

    ' zapewnienie przezywalnosci pomiedzy stronami
    Public Shared mlPociagiStacji As Collection(Of EntryRozkladu) = Nothing
    Public Shared msStacjaNazwa As String = ""
    Public Shared moStacjaDate As DateTime
    Public Shared mbStacjaLastType As Boolean

    Public Shared moPolaczenia As Collection(Of JednoPolaczenie) = Nothing
End Class