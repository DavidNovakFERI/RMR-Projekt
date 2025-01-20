using Microsoft.Maui.Devices.Sensors;

using System;
using System.Linq;
using System.Threading.Tasks;

public class Tracker
{
    private Location previousLocation;
    private DateTime startTime;
    private double totalDistance; // v kilometrih
    private TimeSpan totalTime;

    public Tracker()
    {
        totalDistance = 0;
        totalTime = TimeSpan.Zero;
    }

    // Začetek teka
    public void StartTracking()
    {
        startTime = DateTime.Now;
        previousLocation = null;
        totalDistance = 0;
        totalTime = TimeSpan.Zero;
        StartLocationUpdates();
    }

    // Ustavite spremljanje
    public void StopTracking()
    {
        // Prikaz skupnih podatkov (razdalje in časa)
        Console.WriteLine($"Skupna razdalja: {totalDistance} km");
        Console.WriteLine($"Skupni čas: {totalTime.TotalMinutes} minut");
    }

    // Začne spremljanje GPS lokacije
    private async void StartLocationUpdates()
    {
        try
        {
            // Nastavite interval za posodobitve lokacije
            while (true)
            {
                var currentLocation = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(5)));

                if (currentLocation != null)
                {
                    // Izračunajte razdaljo, če imamo prejšnjo lokacijo
                    if (previousLocation != null)
                    {
                        double distance = Location.CalculateDistance(previousLocation, currentLocation, DistanceUnits.Kilometers);
                        totalDistance += distance;
                        totalTime = DateTime.Now - startTime;

                        // Izračunajte hitrost
                        double speed = distance / totalTime.TotalHours;
                        Console.WriteLine($"Trenutna hitrost: {speed:F2} km/h");
                    }

                    // Posodobi prejšnjo lokacijo
                    previousLocation = currentLocation;

                    // Prikaz trenutnih podatkov
                    Console.WriteLine($"Trenutna razdalja: {totalDistance:F2} km");
                    Console.WriteLine($"Pretekli čas: {totalTime.Minutes} minut");
                }

                await Task.Delay(5000); // Posodobitev vsakih 5 sekund
            }
        }
        catch (FeatureNotSupportedException fnsEx)
        {
            // Lokacija ni podprta
            Console.WriteLine(fnsEx.Message);
        }
        catch (PermissionException pEx)
        {
            // Dovoljenje za dostop do lokacije ni bilo dodeljeno
            Console.WriteLine(pEx.Message);
        }
        catch (Exception ex)
        {
            // Neznana napaka
            Console.WriteLine(ex.Message);
        }
    }
}
