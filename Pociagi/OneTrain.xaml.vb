' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

Imports Windows.Data.Xml.Dom

Public Class EntryPociagu
    Public Property sStacja As String
    Public Property sArrival As String
    Public Property sDeparture As String
    Public Property sKurs As String ' nazwa pociagu, ktora moze sie zmieniac w trakcie
    Public Property sPeron As String    ' czasem nie ma tej kolumny!
    Public Property iColPeron As Integer
    Public Property iColCzas As Integer
End Class

''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
''' 
Public NotInheritable Class OneTrain
    Inherits Page

    Dim msLink As String
    Dim moTrain As Collection(Of EntryPociagu)
    ' tu z "..." polecenie 'obserwuj'

    Protected Overrides Sub onNavigatedTo(e As NavigationEventArgs)
        msLink = e.Parameter.ToString
    End Sub

    Private Sub ImportTrainDataDesktop(sPage As String)
        Dim iInd As Integer
        iInd = sPage.IndexOf("<tbody>")
        If iInd < 10 Then Exit Sub
        Dim sHtml As String = sPage.Substring(iInd)
        iInd = sHtml.IndexOf("</tbody>")
        sHtml = sHtml.Substring(0, iInd + 8)
        sHtml = "<table>" & sHtml & "</table>"

        moTrain = New Collection(Of EntryPociagu)

        iInd = sHtml.IndexOf("<tr")
        While iInd > -1
            sHtml = sHtml.Substring(iInd + 3)

            Dim oItem As EntryPociagu = New EntryPociagu
            oItem.iColCzas = If(App.IsMobile, 20, 50)

            iInd = sHtml.IndexOf("<td") ' kolumna zwin/rozwin? z kulkami?
            sHtml = sHtml.Substring(iInd + 3)

            iInd = sHtml.IndexOf("<td") ' kolumna z przystankiem
            sHtml = sHtml.Substring(iInd)

            iInd = sHtml.IndexOf("<td", 1) ' kolumna z arrival
            oItem.sStacja = Net.WebUtility.HtmlDecode(App.RemoveHtmlTags(sHtml.Substring(0, iInd - 1)))
            sHtml = sHtml.Substring(iInd)

            '<td Class="footable-visible">
            '10:51
            '<span Class="prognosis">
            ' <span Class="rtinfo">ok.&nbsp;+20&nbsp;min.</span>
            '</span>

            iInd = sHtml.IndexOf("<td", 1) ' kolumna z departure
            oItem.sArrival = App.RemoveHtmlTags(sHtml.Substring(0, iInd - 1))
            sHtml = sHtml.Substring(iInd)

            iInd = sHtml.IndexOf("<td", 1) ' kolumna z sKurs
            oItem.sDeparture = App.RemoveHtmlTags(sHtml.Substring(0, iInd - 1))
            sHtml = sHtml.Substring(iInd)

            iInd = sHtml.IndexOf("<t", 1) ' albo <tr, albo <td z peronem
            If iInd > -1 Then

                oItem.sKurs = App.RemoveHtmlTags(sHtml.Substring(0, iInd - 1))

                If iInd = sHtml.IndexOf("<td", 1) Then
                    iInd = sHtml.IndexOf("<t", 2) ' albo <tr, albo <td z peronem
                    oItem.sPeron = App.RemoveHtmlTags(sHtml.Substring(0, iInd - 1))
                End If
                sHtml = sHtml.Substring(iInd)
            End If

            moTrain.Add(oItem)

            iInd = sHtml.IndexOf("<tr")
        End While


    End Sub

    Private Function GlueTwoLines(sLine1 As String, sLine2 As String) As String
        sLine1 = Net.WebUtility.HtmlDecode(App.RemoveHtmlTags(sLine1)).Trim
        sLine2 = Net.WebUtility.HtmlDecode(App.RemoveHtmlTags(sLine2)).Trim
        If sLine1 = "" Then Return sLine2
        If sLine2 = "" Then Return sLine1

        If sLine2 = "✓" Then
            Return sLine1 & sLine2
        Else
            Return sLine1 & vbCrLf & sLine2
        End If
    End Function

    Private Function GlueCzasAndPrognoza(sLine1 As String, sLine2 As String) As String

        If sLine2.IndexOf("rt_on_time") > 1 Then sLine2 = "✓"

        Return GlueTwoLines(sLine1, sLine2)

    End Function

    Private Sub ImportTrainDataMobile(sHtml As String)
        Dim iInd As Integer

        Dim bIsPrognoza As Boolean = False
        If sHtml.IndexOf("Prognoza</th>") > 0 Then bIsPrognoza = True
        Dim sPrognoza As String

        moTrain = New Collection(Of EntryPociagu)

        iInd = sHtml.IndexOf("nowrap sepline")
        While iInd > -1
            sHtml = sHtml.Substring(iInd)
            iInd = sHtml.IndexOf(">")
            sHtml = sHtml.Substring(iInd + 1)

            Dim oItem As EntryPociagu = New EntryPociagu
            oItem.iColCzas = If(App.IsMobile, 50, 50)   ' kiedys, bo było źle?

            iInd = sHtml.IndexOf("<td") ' kolumna z arrival
            oItem.sStacja = Net.WebUtility.HtmlDecode(App.RemoveHtmlTags(sHtml.Substring(0, iInd - 1)))
            sHtml = sHtml.Substring(iInd)

            If bIsPrognoza Then
                ' dodatkowa kolumna
                iInd = sHtml.IndexOf("<td", 1) ' kolumna z prognoza
                sPrognoza = sHtml.Substring(0, iInd - 1)
                sHtml = sHtml.Substring(iInd)
            Else
                sPrognoza = ""
            End If

            iInd = sHtml.IndexOf("<td", 1) ' kolumna z departure
            oItem.sArrival = GlueCzasAndPrognoza(sPrognoza, sHtml.Substring(0, iInd - 1))
            sHtml = sHtml.Substring(iInd)

            If bIsPrognoza Then
                ' dodatkowa kolumna
                iInd = sHtml.IndexOf("<td", 1) ' kolumna z prognoza
                sPrognoza = sHtml.Substring(0, iInd - 1)
                sHtml = sHtml.Substring(iInd)
            Else
                sPrognoza = ""
            End If

            iInd = sHtml.IndexOf("<td", 1) ' kolumna z sKurs
            oItem.sDeparture = GlueCzasAndPrognoza(sPrognoza, sHtml.Substring(0, iInd - 1))
            sHtml = sHtml.Substring(iInd)

            iInd = sHtml.IndexOf("<t", 1) ' albo <tr, albo <td z peronem
            If iInd > -1 Then

                oItem.sKurs = App.RemoveHtmlTags(sHtml.Substring(0, iInd - 1))

                If iInd = sHtml.IndexOf("<td", 1) Then
                    iInd = sHtml.IndexOf("<t", 2) ' albo <tr, albo <td z peronem
                    oItem.sPeron = App.RemoveHtmlTags(sHtml.Substring(0, iInd - 1))
                End If
                sHtml = sHtml.Substring(iInd)
            End If

            moTrain.Add(oItem)

            iInd = sHtml.IndexOf("nowrap sepline")
        End While


    End Sub

    Private Async Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        Dim sPage As String = Await App.HttpPageAsync(msLink, "train data")
        If sPage = "" Then Exit Sub

        Dim bMobileVers As Boolean = True
        If msLink.IndexOf("mobil.") < 0 Then bMobileVers = False

        If bMobileVers Then
            ImportTrainDataMobile(sPage)
        Else
            ImportTrainDataDesktop(sPage)
        End If

        If moTrain.Count < 1 Then
            App.DialogBox("ERROR reading train data")
            Exit Sub
        End If

        '    If moTrain.Count > journeyStartIdx=12&journeyEndIdx=19 z msLink
        ' albo nie, bo pokazuje cały pociąg?
        ' w moTrain albo gdzies: current delay

        uiTitle.Text = moTrain.Item(0).sStacja & " - " & moTrain.Item(moTrain.Count - 1).sStacja

            Dim iInd As Integer

            If bMobileVers Then
                iInd = sPage.IndexOf("Dalsze informacje:")
            Else
                iInd = sPage.IndexOf("more-info-header")
            End If

            If iInd > 10 Then
                iInd = sPage.LastIndexOf("<", iInd)
                Dim sTmp As String
                sTmp = sPage.Substring(iInd)
                If bMobileVers Then
                    iInd = sTmp.IndexOf("</td>")
                Else
                    iInd = sTmp.IndexOf("</div")
                End If
                uiAddInfo.Text = Net.WebUtility.HtmlDecode(App.RemoveHtmlTags(sTmp.Substring(0, iInd - 1))).Replace(vbLf & vbLf, vbCrLf)
            End If

            If msLink = App.GetSettingsString("CurrTrainLink") Then
                uiMonitorTrain.IsEnabled = True
            Else
                uiMonitorTrain.IsEnabled = False
            End If

            ShowRemovePeron()

            uiListItems.ItemsSource = moTrain

    End Sub

    Private Function ShowRemovePeron() As Boolean
        If moTrain Is Nothing Then Return False
        If moTrain.Count = 0 Then Return False

        Dim iTmp As Integer = 70
        If uiPage.ActualWidth < 510 Then iTmp = 0  ' Lumia 435: = 480
        Dim iPrev As Integer = moTrain.Item(0).iColPeron
        If iPrev = iTmp Then Return False

        For Each oItem As EntryPociagu In moTrain
            oItem.iColPeron = iTmp
        Next

        Return True
    End Function

    Private Sub uiShowInfo_Click(sender As Object, e As RoutedEventArgs) Handles uiShowInfo.Click
        If uiShowInfo.IsChecked Then
            uiAddInfo.Visibility = Visibility.Visible
        Else
            uiAddInfo.Visibility = Visibility.Collapsed
        End If
    End Sub

    Private Sub uiMonitorTrain_Click(sender As Object, e As RoutedEventArgs) Handles uiMonitorTrain.Click
        App.DialogBox("Unimplemented yet - co 15 minut kontrola opoznienia na końcu")
    End Sub

    Private Sub uiSetTrain_Click(sender As Object, e As RoutedEventArgs)
        App.SetSettingsString("CurrTrainLink", msLink)
        App.SetSettingsString("CurrTrainName", uiTitle.Text)
        App.SetSettingsInt("CurrTrainSetTime", Date.Now.DayOfYear)
        uiMonitorTrain.IsEnabled = True
    End Sub

    Private Sub Page_SizeChanged(sender As Object, e As SizeChangedEventArgs)
        If ShowRemovePeron() Then uiListItems.ItemsSource = moTrain
    End Sub

    Private Sub uiPokazRozklad_Click(sender As Object, e As RoutedEventArgs)
        Dim sName As String = TryCast(TryCast(sender, MenuFlyoutItem).DataContext, EntryPociagu).sStacja
        App.mlPociagiStacji = Nothing   ' zeby na pewno wczytał, nie korzystał z cache
        Me.Frame.Navigate(GetType(RozkladStacji), sName)
    End Sub

    Private Sub uiGoAtlas_Click(sender As Object, e As RoutedEventArgs)
        '       "Przejdź do atlaskolejowy" (do wyszukiwarki?)   
        '           http://pl.atlaskolejowy.net/mazowieckie/?id=baza&poz=3980
        '           busko: http://pl.atlaskolejowy.net/swietokrzyskie/?id=baza&poz=2522
        '               (ale mozna wykasowac 'swietokrzyskie' i tez zadziała)

        Dim iId As Integer
        Dim oItem As EntryPociagu = TryCast(TryCast(sender, MenuFlyoutItem).DataContext, EntryPociagu)

        Dim sName As String = oItem.sStacja
        iId = App.moPunkty.GetAtlasId(sName)

        If iId = 0 Then
            ' nie znamy identyfikatora, strona szukania
            ' *TODO* to nie dziala, tam jest POST
            App.OpenBrowser("http://pl.atlaskolejowy.net/wynik.php?x=1&y=1&szukaj=" & sName, False)
        Else
            App.OpenBrowser("http://pl.atlaskolejowy.net/?id=baza&poz=" & iId, False)
        End If

    End Sub

    Private Sub uiGoBaza_Click(sender As Object, e As RoutedEventArgs)
        '       "Przejdź do bazakolejowa" (do wyszukiwarki? bo skąd miałbym id?)
        '           https://www.bazakolejowa.pl/index.php?dzial=stacje&id=7358&okno=start
        '           busko: https://www.bazakolejowa.pl/index.php?dzial=stacje&id=636&okno=start

        Dim iId As Integer
        Dim oItem As EntryPociagu = TryCast(TryCast(sender, MenuFlyoutItem).DataContext, EntryPociagu)

        Dim sName As String = oItem.sStacja
        iId = App.moPunkty.GetBazaId(sName)

        If iId = 0 Then
            ' nie znamy identyfikatora, strona szukania
            App.OpenBrowser("https://www.bazakolejowa.pl/index.php?dzial=szukaj&rodzaj=s&szukaj=" & Uri.EscapeUriString(sName), False)
        Else
            App.OpenBrowser("https://www.bazakolejowa.pl/index.php?dzial=stacje&id=" & iId, False)
        End If

    End Sub

    Private Sub uiRefresh_Click(sender As Object, e As RoutedEventArgs)
        Page_Loaded(Nothing, Nothing)
    End Sub
End Class
