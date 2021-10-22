' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class MainPage
    Inherits Page

    Private Async Sub bRateIt_Click(sender As Object, e As RoutedEventArgs)
        Dim sUri As New Uri("ms-windows-store://review/?PFN=" & Package.Current.Id.FamilyName)
        Await Windows.System.Launcher.LaunchUriAsync(sUri)

    End Sub

    Private Sub uiSearchStacja_Click(sender As Object, e As RoutedEventArgs) Handles uiSearchStacja.Click
        Me.Frame.Navigate(GetType(Stacje))
    End Sub

    Private Sub uiSearchRoute_Click(sender As Object, e As RoutedEventArgs) Handles uiSearchRoute.Click
        Me.Frame.Navigate(GetType(SzukajPociagu))
    End Sub

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        If Math.Abs(App.GetSettingsInt("CurrTrainSetTime") - Date.Now.DayOfYear) > 2 Then
            uiLastRoute.IsEnabled = False
        Else
            uiLastRoute.IsEnabled = True
            uiLastRoute.Content = App.GetSettingsString("CurrTrainName")
        End If

    End Sub

    Private Sub uiLastRoute_Click(sender As Object, e As RoutedEventArgs) Handles uiLastRoute.Click
        Me.Frame.Navigate(GetType(OneTrain), App.GetSettingsString("CurrTrainLink"))
    End Sub

    Private Sub uiSettings_Click(sender As Object, e As RoutedEventArgs)
        Me.Frame.Navigate(GetType(Settings))
    End Sub

    Private Sub uiSygnaly_Click(sender As Object, e As RoutedEventArgs) Handles uiSygnaly.Click
        Me.Frame.Navigate(GetType(Semafory))
    End Sub
End Class
