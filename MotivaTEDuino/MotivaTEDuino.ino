/*
HealthBear.ino

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

#include <Wire.h>
#include "rgb_lcd.h"
#include "SoftwareSerial.h"

// PINS
// constants
const int TX_BT = 10;
const int RX_BT = 11;

// Messaging
const unsigned long periodicMessageFrequency = 1000; //Frequency for periodic messages [milliseconds]
unsigned long time = 0;

// Workforce
SoftwareSerial btSerial(TX_BT, RX_BT);

rgb_lcd lcd;

void setup()
{
	Serial.begin(9600);
	btSerial.begin(9600);

	// set up the LCD's number of columns and rows:
	lcd.begin(16, 2);

	delay(1000);
}

void loop()
{
	bluetoothHandler();		// Handle incoming bluetooth communication
}

// Handler for incoming bluetooth communication
void bluetoothHandler()
{

	// Make sure bluetooth is available
	if (btSerial.available())
	{
		int commandSize = 0;
		commandSize = (int) btSerial.read();
		char command[commandSize];
		int commandPos = 0;
		while (commandPos < commandSize)
		{
			if (btSerial.available())
			{
				command[commandPos] = (char) btSerial.read();
				commandPos++;
			}
		}
		command[commandPos] = 0;
		//Process command
		inboundHandler(command);
	}
}

void inboundHandler(char* message)
{
	String fullMessage = (String) message;

	int commaIndex = fullMessage.indexOf('-');
	//  Search for the next comma just after the first
	int secondCommaIndex = fullMessage.indexOf('-', commaIndex + 1);
	int thirdCommaIndex = fullMessage.indexOf('-', secondCommaIndex + 1);

	String heartRate = fullMessage.substring(0, commaIndex);
	String red = fullMessage.substring(commaIndex + 1, secondCommaIndex);
	String green = fullMessage.substring(secondCommaIndex + 1, thirdCommaIndex);
	String blue = fullMessage.substring(thirdCommaIndex);// To the end of the string

	int r = red.toInt();
	int g = green.toInt();
	int b = blue.toInt();

	lcd.setCursor(0, 1);
	lcd.print("Heart Rate: " + heartRate);
	//Set color based on Heart Rate

	lcd.setRGB(r, g, b);

}
