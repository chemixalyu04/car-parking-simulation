Imports System.Windows
Imports System.Windows.Input
Imports System.Windows.Shapes

Namespace CarParkingProject
    Public Class MainWindow
        Private isDragging As Boolean = False
        Private draggedCar As Rectangle
        Private startPos As Point
        ' To track which car is parked in which slot
        Private parkedCars As New Dictionary(Of Rectangle, Rectangle)()

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub Car_MouseDown(sender As Object, e As MouseButtonEventArgs)
            draggedCar = CType(sender, Rectangle)
            isDragging = True
            startPos = e.GetPosition(ParkingCanvas)
            draggedCar.CaptureMouse()
        End Sub

        Private Sub ParkingCanvas_MouseMove(sender As Object, e As MouseEventArgs)
            If isDragging AndAlso draggedCar IsNot Nothing Then
                Dim mousePos As Point = e.GetPosition(ParkingCanvas)
                Canvas.SetLeft(draggedCar, mousePos.X - (draggedCar.Width / 2))
                Canvas.SetTop(draggedCar, mousePos.Y - (draggedCar.Height / 2))
            End If
        End Sub

        Private Sub ParkingCanvas_MouseUp(sender As Object, e As MouseButtonEventArgs)
            If isDragging AndAlso draggedCar IsNot Nothing Then
                Dim mousePos As Point = e.GetPosition(ParkingCanvas)

                ' Check if the car is over a slot to drop it there
                If IsMouseOverSlot(mousePos, Slot1) Then
                    HandleParking(draggedCar, Slot1, Status1)
                ElseIf IsMouseOverSlot(mousePos, Slot2) Then
                    HandleParking(draggedCar, Slot2, Status2)
                ElseIf IsMouseOverSlot(mousePos, Slot3) Then
                    HandleParking(draggedCar, Slot3, Status3)
                Else
                    ' Reset car position if not parked in any slot
                    ResetCarPosition(draggedCar)
                    RemoveCarFromSlot(draggedCar)
                End If

                draggedCar.ReleaseMouseCapture()
                isDragging = False
                draggedCar = Nothing
            End If
        End Sub

        Private Sub HandleParking(car As Rectangle, slot As Rectangle, statusText As TextBlock)
            ' If the slot is already occupied, reset the previously parked car's position
            Dim previousCar As Rectangle = Nothing
            If parkedCars.ContainsKey(slot) Then
                previousCar = parkedCars(slot)
                ResetCarPosition(previousCar)
            End If

            ' Park the new car and update the slot's status
            Canvas.SetLeft(car, Canvas.GetLeft(slot) + (slot.Width - car.Width) / 2)
            Canvas.SetTop(car, Canvas.GetTop(slot) + (slot.Height - car.Height) / 2)
            statusText.Text = "Occupied"

            ' Update parkedCars dictionary
            If previousCar IsNot Nothing Then
                parkedCars.Remove(slot) ' Remove previous car from the slot
            End If
            parkedCars(slot) = car ' Update to new car in this slot
        End Sub

        Private Sub RemoveCarFromSlot(car As Rectangle)
            ' Check each slot for the removed car
            Dim slotToRemove As Rectangle = Nothing
            For Each slot In parkedCars.Keys
                If parkedCars(slot) Is car Then
                    slotToRemove = slot
                    Exit For
                End If
            Next

            ' If we found the slot where the car was parked, update the status
            If slotToRemove IsNot Nothing Then
                If slotToRemove Is Slot1 Then Status1.Text = "Available"
                If slotToRemove Is Slot2 Then Status2.Text = "Available"
                If slotToRemove Is Slot3 Then Status3.Text = "Available"
                parkedCars.Remove(slotToRemove) ' Remove car from tracking
            End If
        End Sub

        Private Sub ResetCarPosition(car As Rectangle)
            ' Reset car to its original position based on the car's name
            Select Case car.Name
                Case "Car1"
                    Canvas.SetLeft(car, 10)
                    Canvas.SetTop(car, 200)
                Case "Car2"
                    Canvas.SetLeft(car, 10)
                    Canvas.SetTop(car, 230)
                Case "Car3"
                    Canvas.SetLeft(car, 10)
                    Canvas.SetTop(car, 260)
                Case "Car4"
                    Canvas.SetLeft(car, 10)
                    Canvas.SetTop(car, 290)
                Case "Car5"
                    Canvas.SetLeft(car, 10)
                    Canvas.SetTop(car, 320)
            End Select
        End Sub

        Private Function IsMouseOverSlot(mousePos As Point, slot As Rectangle) As Boolean
            Dim slotLeft As Double = Canvas.GetLeft(slot)
            Dim slotTop As Double = Canvas.GetTop(slot)
            Return mousePos.X >= slotLeft AndAlso mousePos.X <= slotLeft + slot.Width AndAlso
                   mousePos.Y >= slotTop AndAlso mousePos.Y <= slotTop + slot.Height
        End Function
    End Class
End Namespace
