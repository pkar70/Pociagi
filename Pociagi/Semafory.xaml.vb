' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

Imports Windows.UI
''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class Semafory
    Inherits Page

    Private moTimer As DispatcherTimer
    Private mbColor As Boolean = True
    Private moBlack As Brush = New SolidColorBrush(Color.FromArgb(255, 32, 32, 32))
    Private moGreen As Brush = New SolidColorBrush(Color.FromArgb(255, 64, 255, 64))   ' FF40FF40
    Private moYellow As Brush = New SolidColorBrush(Color.FromArgb(255, 255, 220, 64))  ' FFFFDC40

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        moTimer = New DispatcherTimer
        moTimer.Interval = TimeSpan.FromSeconds(0.5)
        AddHandler moTimer.Tick, AddressOf Timer_Tick
        moTimer.Start()

    End Sub

    Private Sub Page_LostFocus(sender As Object, e As RoutedEventArgs)
        moTimer.Stop()
        moTimer = Nothing
    End Sub

    Private Sub Timer_Tick()
        If mbColor Then
            uiGreenFlash.Fill = moBlack
            uiYellowFlash.Fill = moBlack
            mbColor = False
        Else
            uiGreenFlash.Fill = moGreen
            uiYellowFlash.Fill = moYellow
            mbColor = True
        End If
    End Sub
End Class
