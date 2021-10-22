' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class Settings
    Inherits Page

    Private Sub uiSave_Click(sender As Object, e As RoutedEventArgs)
        App.SetSettingsInt("nearestRadix", uiOdl.Value)
        App.SetSettingsBool("autoWpisNazwy", uiAutoComplete.IsOn)
        App.SetSettingsBool("stacjePosrednieRozklad", uiStacjePosrednieRozklad.IsOn)
        App.SetSettingsBool("nazwyOnline", uiNazwyOnline.IsOn)
    End Sub

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        Dim iRadix As Integer = App.GetSettingsInt("nearestRadix", 5)
        If iRadix > 10 Then iRadix = 10
        uiOdl.Value = iRadix

        uiAutoComplete.IsOn = App.GetSettingsBool("autoWpisNazwy")
        uiStacjePosrednieRozklad.IsOn = App.GetSettingsBool("stacjePosrednieRozklad")
        uiNazwyOnline.IsOn = App.GetSettingsBool("nazwyOnline")

        If Not App.IsMobile Then
            ' usuwamy rzeczy tylko mobilne
            uiStacjePosrednieRozklad.Visibility = Visibility.Collapsed
        End If
    End Sub
End Class
