Imports System.Windows
Imports System.Windows.Input
Imports System.Windows.Shapes
Imports System.Windows.Threading
Imports System.Linq
Imports System.Collections.Generic
Imports System.Windows.Controls.Primitives

Namespace CarParkingProject
    Public Class MainWindow
        Private isDragging As Boolean = False
        Private draggedCar As Rectangle
        Private dragOffset As Point
        Private parkingDuration As Dictionary(Of Rectangle, DateTime)
        Private vehicleBudgets As Dictionary(Of Rectangle, Decimal)
        Private Const CarFeePerMinute As Decimal = 0.33D ' Approx 20 PHP per hour
        Private Const MotorcycleFeePerMinute As Decimal = 0.17D ' Approx 10 PHP per hour
        Private DispatcherTimer As DispatcherTimer
        Private vehicles As List(Of Vehicle)
        Private carModels As String() = {"Toyota", "Honda", "Ford", "Chevrolet", "Nissan", "BMW", "Mercedes", "Audi", "Volkswagen", "Hyundai"}

        Public Sub New()
            InitializeComponent()
            vehicles = New List(Of Vehicle)()
            InitializeVehicles()
            InitializeParkingDurations()
            InitializeBudgets()

            ' Initialize the timer for fee calculation
            DispatcherTimer = New DispatcherTimer()
            AddHandler DispatcherTimer.Tick, AddressOf Timer_Tick
            DispatcherTimer.Interval = TimeSpan.FromMinutes(1) ' Update every minute
            DispatcherTimer.Start()
        End Sub

        Private Sub InitializeVehicles()
            ' Pre-assign vehicle data
            vehicles.Add(New Vehicle("Car A", "Blue", False, Car1, carModels))
            vehicles.Add(New Vehicle("Car B", "Red", False, Car2, carModels))
            vehicles.Add(New Vehicle("Car C", "Green", False, Car3, carModels))
            vehicles.Add(New Vehicle("Car D", "Yellow", False, Car4, carModels))
            vehicles.Add(New Vehicle("Car E", "Purple", False, Car5, carModels))
            vehicles.Add(New Vehicle("Motorcycle A", "Cyan", True, Motor1, carModels))
            vehicles.Add(New Vehicle("Motorcycle B", "Indigo", True, Motor2, carModels))
            vehicles.Add(New Vehicle("Motorcycle C", "Brown", True, Motor3, carModels))
            vehicles.Add(New Vehicle("Motorcycle D", "Orange", True, Motor4, carModels))
        End Sub

        Private Sub InitializeParkingDurations()
            parkingDuration = New Dictionary(Of Rectangle, DateTime)() From {
                {Car1, DateTime.Now}, {Car2, DateTime.Now}, {Car3, DateTime.Now},
                {Car4, DateTime.Now}, {Car5, DateTime.Now},
                {Motor1, DateTime.Now}, {Motor2, DateTime.Now},
                {Motor3, DateTime.Now}, {Motor4, DateTime.Now}
            }
        End Sub

        Private Sub InitializeBudgets()
            vehicleBudgets = New Dictionary(Of Rectangle, Decimal)() From {
                {Car1, 100D}, {Car2, 100D}, {Car3, 100D}, {Car4, 100D}, {Car5, 100D},
                {Motor1, 100D}, {Motor2, 100D}, {Motor3, 100D}, {Motor4, 100D}
            }
        End Sub

        Private Sub Car_MouseDown(sender As Object, e As MouseButtonEventArgs)
            draggedCar = DirectCast(sender, Rectangle)
            dragOffset = e.GetPosition(draggedCar)
            isDragging = True
            DetailBox.Visibility = Visibility.Visible
            ShowCarDetails(draggedCar)
        End Sub

        Private Sub ShowCarDetails(car As Rectangle)
            Dim vehicle = vehicles.FirstOrDefault(Function(v) v.CarRectangle Is car)
            If vehicle IsNot Nothing Then
                CarDetails.Text = $"{vehicle.Name} - Color: {vehicle.Color}" & vbCrLf &
                                  $"Type: {(If(vehicle.IsMotorcycle, "Motorcycle", "Car"))}" & vbCrLf &
                                  $"Budget: {vehicleBudgets(car):C}"
            End If
        End Sub

        Private Sub ParkingCanvas_MouseMove(sender As Object, e As MouseEventArgs)
            If isDragging AndAlso draggedCar IsNot Nothing Then
                Dim mousePos As Point = e.GetPosition(ParkingCanvas)
                Canvas.SetLeft(draggedCar, mousePos.X - dragOffset.X)
                Canvas.SetTop(draggedCar, mousePos.Y - dragOffset.Y)
            End If
        End Sub

        Private Sub ParkingCanvas_MouseUp(sender As Object, e As MouseButtonEventArgs)
            If isDragging Then
                isDragging = False
                HandleParking(draggedCar)
                draggedCar = Nothing
                DetailBox.Visibility = Visibility.Hidden
            End If
        End Sub

        Private Sub HandleParking(car As Rectangle)
            Dim carSlot As Rectangle = GetParkingSlot(car)
            If carSlot IsNot Nothing Then
                Dim statusTextBlock = GetStatusTextBlock(carSlot)
                If statusTextBlock IsNot Nothing Then
                    Dim vehiclesInSlot As List(Of Vehicle) = GetVehiclesInSlot(carSlot)
                    If vehiclesInSlot.Count = 1 AndAlso vehiclesInSlot.First().IsMotorcycle Then
                        statusTextBlock.Text = "Available"
                    ElseIf vehiclesInSlot.Count = 1 AndAlso Not vehiclesInSlot.First().IsMotorcycle Then
                        statusTextBlock.Text = "Occupied"
                    ElseIf vehiclesInSlot.Count = 2 AndAlso vehiclesInSlot.All(Function(v) v.IsMotorcycle) Then
                        statusTextBlock.Text = "Occupied"
                    Else
                        statusTextBlock.Text = "Available"
                    End If
                End If
            End If
        End Sub

        Private Function GetParkingSlot(car As Rectangle) As Rectangle
            Select Case car.Name
                Case "Car1"
                    Return Slot1
                Case "Car2"
                    Return Slot2
                Case "Car3"
                    Return Slot3
                Case "Car4"
                    Return Slot4
                Case "Car5"
                    Return Slot5
                Case "Motor1", "Motor2", "Motor3", "Motor4"
                    Return Slot1 ' Adjust this if you have separate motorcycle slots
                Case Else
                    Return Nothing
            End Select
        End Function

        Private Function GetStatusTextBlock(slot As Rectangle) As TextBlock
            Select Case slot.Name
                Case "Slot1"
                    Return Status1
                Case "Slot2"
                    Return Status2
                Case "Slot3"
                    Return Status3
                Case "Slot4"
                    Return Status4
                Case "Slot5"
                    Return Status5
                Case Else
                    Return Nothing
            End Select
        End Function

        Private Function GetVehiclesInSlot(slot As Rectangle) As List(Of Vehicle)
            Return vehicles.Where(Function(v) GetParkingSlot(v.CarRectangle) Is slot).ToList()
        End Function

        Private Sub Timer_Tick(sender As Object, e As EventArgs)
            For Each vehicle In vehicles
                Dim parkedDuration = DateTime.Now - parkingDuration(vehicle.CarRectangle)
                If parkedDuration.TotalMinutes >= 1 Then
                    UpdateBudget(vehicle.CarRectangle, parkedDuration.TotalMinutes)
                End If
            Next
        End Sub

        Private Sub UpdateBudget(car As Rectangle, parkedMinutes As Double)
            If vehicleBudgets.ContainsKey(car) Then
                Dim budget = vehicleBudgets(car)
                If budget > 0 Then
                    Dim cost = If(vehicles.First(Function(v) v.CarRectangle Is car).IsMotorcycle, MotorcycleFeePerMinute, CarFeePerMinute) * parkedMinutes
                    budget -= cost
                    vehicleBudgets(car) = budget
                    FeeDisplay.Text = $"{GetVehicleName(car)}'s Budget: {budget:C}"

                    If budget <= 0 Then
                        MessageBox.Show($"{GetVehicleName(car)} has run out of budget!")
                        HandleVehicleRemoval(car)
                    End If
                End If
            End If
        End Sub

        Private Function GetVehicleName(car As Rectangle) As String
            Dim vehicle = vehicles.FirstOrDefault(Function(v) v.CarRectangle Is car)
            Return If(vehicle IsNot Nothing, vehicle.Name, "Unknown Vehicle")
        End Function

        Private Sub HandleVehicleRemoval(car As Rectangle)
            Dim slot = GetParkingSlot(car)
            If slot IsNot Nothing Then
                Dim statusTextBlock = GetStatusTextBlock(slot)
                If statusTextBlock IsNot Nothing Then
                    Dim vehiclesInSlot As List(Of Vehicle) = GetVehiclesInSlot(slot)
                    If vehiclesInSlot.Count = 1 AndAlso vehiclesInSlot.First().IsMotorcycle Then
                        statusTextBlock.Text = "Available"
                    ElseIf vehiclesInSlot.Count = 1 AndAlso Not vehiclesInSlot.First().IsMotorcycle Then
                        statusTextBlock.Text = "Occupied"
                    ElseIf vehiclesInSlot.Count = 2 AndAlso vehiclesInSlot.All(Function(v) v.IsMotorcycle) Then
                        statusTextBlock.Text = "Occupied"
                    Else
                        statusTextBlock.Text = "Available"
                    End If
                End If
            End If
        End Sub

        Private Sub ResetButton_Click(sender As Object, e As RoutedEventArgs)
            For Each vehicle In vehicles
                vehicleBudgets(vehicle.CarRectangle) = 100D ' Reset budgets to 100 PHP
                Dim statusTextBlock = GetStatusTextBlock(vehicle.CarRectangle)
                If statusTextBlock IsNot Nothing Then
                    statusTextBlock.Text = "Available"
                End If
                FeeDisplay.Text = ""
            Next
        End Sub

        ' New method for handling the parking fee payment
        Private Sub PayParkingFeeButton_Click(sender As Object, e As RoutedEventArgs)
            For Each vehicle In vehicles
                Dim car As Rectangle = vehicle.CarRectangle
                If vehicleBudgets.ContainsKey(car) Then
                    Dim budget = vehicleBudgets(car)
                    If budget > 0 Then
                        Dim cost = If(vehicle.IsMotorcycle, MotorcycleFeePerMinute, CarFeePerMinute)
                        budget -= cost
                        vehicleBudgets(car) = budget
                        FeeDisplay.Text = $"{vehicle.Name}'s Budget: {budget:C}"

                        If budget <= 0 Then
                            MessageBox.Show($"{vehicle.Name} has run out of budget!")
                            HandleVehicleRemoval(car)
                        End If
                    End If
                End If
            Next
        End Sub

    End Class

    Public Class Vehicle
        Public Property Name As String
        Public Property Color As String
        Public Property IsMotorcycle As Boolean
        Public Property CarRectangle As Rectangle
        Public Property AvailableModels As String()

        Public Sub New(name As String, color As String, isMotorcycle As Boolean, carRectangle As Rectangle, availableModels As String())
            Me.Name = name
            Me.Color = color
            Me.IsMotorcycle = isMotorcycle
            Me.CarRectangle = carRectangle
            Me.AvailableModels = availableModels
        End Sub
    End Class
End Namespace
