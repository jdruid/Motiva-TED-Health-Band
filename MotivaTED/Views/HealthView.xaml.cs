// Copyright (c) Microsoft Corporation
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Storage;  //Application Data
using Windows.UI.Core;
using System.Threading;

using Microsoft.Band.Sensors;   //Band

using MotivaTED.Band.Model; //Models

namespace MotivaTED.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HealthView : Page
    {

        private StreamingModel model;
        private int age;

        public HealthView()
        {
            //default
            age = 100;

            this.InitializeComponent();

            this.model = new StreamingModel();
            this.DataContext = this.model;
            //Handle the binding of the "links"
            ScenarioControl.ItemsSource = MainPage.Current.Scenarios;
            //Stream handler for the band
            HRStreaming_Handler();
        }

        /// <summary>
        /// Handles streaming of HR Rate from the MS Band
        /// </summary>
        private async void HRStreaming_Handler()
        {
            if (model.Main.BandClient != null)
            {

                using (new DisposableAction(() => model.StreamingBusy = true, () => model.StreamingBusy = false))
                {
                    if (HRStreaming.Visibility == Visibility.Collapsed)
                    {
                        //subscribe to HR
                        model.Main.BandClient.SensorManager.HeartRate.ReadingChanged += HeartRate_ReadingChanged;
                        await model.Main.BandClient.SensorManager.HeartRate.StartReadingsAsync(new CancellationToken());

                        HRStreaming.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        //unsubscribe to HR
                        model.Main.BandClient.SensorManager.HeartRate.ReadingChanged -= HeartRate_ReadingChanged;
                        await model.Main.BandClient.SensorManager.HeartRate.StopReadingsAsync(new CancellationToken());

                        HRStreaming.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private async void HeartRate_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandHeartRateReading> e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                PerformHealthCheck(e.SensorReading.HeartRate);

            }).AsTask();

        }

        private void PerformHealthCheck(int heartRate)
        {
            //Update model
            model.HeartRate = heartRate;
            //Change color of text
            SolidColorBrush heartRateColor = DetermineColor(heartRate, age);
            HearRateTextBlock.Foreground = heartRateColor;

            //Send message
            if (App.BluetoothManager.State == ArduinoBluetooth.Connection.BluetoothConnectionState.Connected)
            {
                //build message to send
                String message = String.Format("{0}-{1}-{2}-{3}", heartRate, heartRateColor.Color.R, heartRateColor.Color.G, heartRateColor.Color.B);
                SendMessage(message);
            }
        }

        /// <summary>
        /// Helper Method to determine color of text
        /// </summary>
        /// <param name="heartRate"></param>
        /// <param name="theAge"></param>
        /// <returns></returns>
        private SolidColorBrush DetermineColor(int heartRate, int theAge)
        {
            //MAX is 220 - age
            //Zone is 50% to 85% of that
            int maxHeartRate = 220 - theAge;
            double zoneLowEnd = maxHeartRate * .50;
            double zoneHighEnd = maxHeartRate * .85;

            if (heartRate <= zoneLowEnd) //Not in the "Zone"
            {
                //Standard Color
                return new SolidColorBrush(Windows.UI.Colors.White);
            }
            else if ((heartRate >= zoneLowEnd) && (heartRate <= zoneHighEnd))
            {
                //burning
                return new SolidColorBrush(Windows.UI.Colors.Green);
            }
            else if (heartRate >= zoneHighEnd)
            {
                //dying
                return new SolidColorBrush(Windows.UI.Colors.Red);
            }
            else
            {
                //really dying
                return new SolidColorBrush(Windows.UI.Colors.OrangeRed);
            }
        }

        /// <summary>
        /// Sends message via bluetooth to arduino
        /// </summary>
        /// <param name="message"></param>
        private async void SendMessage(String message)
        {
            var res = await App.BluetoothManager.SendMessageAsync(message);
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("Age"))
            {
                age = (int)(ApplicationData.Current.LocalSettings.Values["Age"]);
            }

            //Configured Age
            if (age < 100) //default is 100
            {
                Step1Done.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            //Configured band
            if (model.Main.BandClient != null)
            {
                Step2Done.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            //Configure teddy
            if (App.BluetoothManager.State == ArduinoBluetooth.Connection.BluetoothConnectionState.Connected)
            {
                Step3Done.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        private void ScenarioControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox scenarioListBox = sender as ListBox;
            Scenario s = scenarioListBox.SelectedItem as Scenario;

            if (s != null)
            {
                Frame scenarioFrame = MainPage.Current.FindName("ScenarioFrame") as Frame;
                scenarioFrame.Navigate(s.ClassType);
            }

            // Clear the selection before we navigate away
            scenarioListBox.SelectedItem = null;
        }
               
    }

    public class ScenarioBindingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Scenario s = value as Scenario;
            return s.ImageLocation;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return true;
        }
    }
}
