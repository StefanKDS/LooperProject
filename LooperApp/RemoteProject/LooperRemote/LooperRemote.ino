#include <BleKeyboard.h>
#include <Keypad.h>


// Globale Variablen /////////////////////////////////////////////////////////////
const byte COLS = 4; //4 Spalten
const byte ROWS = 4; //4 Zeilen

///////////////////////////////////////////////////////////////////////////////

//Die Ziffern und Zeichen des Keypads werden eingegeben:
char hexaKeys[ROWS][COLS]={
{'1','4','7','*'},
{'2','5','8','0'},
{'3','6','9','#'},
{'A','B','C','D'}
};

byte colPins[COLS] = {23,22,3,21}; //Definition der Pins für die 4 Spalten
byte rowPins[ROWS] = {19,18,5,17};//Definition der Pins für die 4 Zeilen
char Taste; //Taste ist die Variable für die jeweils gedrückte Taste.

const byte led_blue_gpio = 32;
const byte led_green_gpio = 33;

char rightButton;
char leftButton;
char remoteRightButton;
char remoteLeftButton;

int buttonMode = 0;

bool isEditing = false;

// Objekte ///////////////////////////////////////////////////////////////////////
Keypad Tastenfeld = Keypad(makeKeymap(hexaKeys), rowPins, colPins, ROWS, COLS); //Das Keypad kann absofort mit "Tastenfeld" angesprochen werden
BleKeyboard bleKeyboard("PageChanger");

// Defines //////////////////////////////////////////////////////////////////////
#define LEFT_BUTTON_PIN 12
#define RIGHT_BUTTON_PIN 13
#define REMOTE_RIGHT_BUTTON_PIN 14
#define REMOTE_LEFT_BUTTON_PIN 27


// Funktionen ///////////////////////////////////////////////////////////////

///////////////////////////////////////////////
// SETUP
//////////////////////////////////////////////

void setup() 
{
    // configure pin for button
    pinMode(RIGHT_BUTTON_PIN, INPUT_PULLUP);
    pinMode(LEFT_BUTTON_PIN, INPUT_PULLUP);
    pinMode(REMOTE_RIGHT_BUTTON_PIN, INPUT_PULLUP);
    pinMode(REMOTE_LEFT_BUTTON_PIN, INPUT_PULLUP);
    pinMode(led_blue_gpio, OUTPUT);
    pinMode(led_green_gpio, OUTPUT);
    Serial.begin(115200);
    Serial.println("Starting BLE work!");
    bleKeyboard.begin();
    digitalWrite(led_green_gpio, LOW); 

    rightButton = KEY_PAGE_UP;
    leftButton = KEY_PAGE_DOWN;

    remoteRightButton = 'c';
    remoteLeftButton = 'd';
    buttonMode = 1;
}

///////////////////////////////////////////////
// CHANGE PAGE MODE
//////////////////////////////////////////////

void ChangePageMode()
{
    rightButton = KEY_PAGE_UP;
    leftButton = KEY_PAGE_DOWN;
}

///////////////////////////////////////////////
// MNUSIC MODE
//////////////////////////////////////////////

void MusicMode()
{
    rightButton = 'a';
    leftButton = 'b';
    remoteRightButton = 'c';
    remoteLeftButton = 'd';
}

///////////////////////////////////////////////
// SHOW MODE
//////////////////////////////////////////////

void ShowMode()
{
    for (int i=0; i < buttonMode; i++)
    {
        digitalWrite(led_green_gpio, HIGH); 
        delay(800);
        digitalWrite(led_green_gpio, LOW); 
        delay(800);
    }
}

///////////////////////////////////////////////
// MAIN LOOP
//////////////////////////////////////////////

void loop() 
{
  // Bluetooth
    if(bleKeyboard.isConnected()) 
    {
        digitalWrite(led_blue_gpio, HIGH); 
        if(digitalRead(RIGHT_BUTTON_PIN) == LOW)
        {
          Serial.println("right");
            bleKeyboard.write(rightButton);
            delay(300);
        }

        if(digitalRead(LEFT_BUTTON_PIN) == LOW)
        {
          Serial.println("left");
            bleKeyboard.write(leftButton);
            delay(300);
        }

        if(digitalRead(REMOTE_RIGHT_BUTTON_PIN) == LOW)
        {
          Serial.println("Remote right");
            bleKeyboard.write(remoteRightButton);
            delay(300);
        }

        if(digitalRead(REMOTE_LEFT_BUTTON_PIN) == LOW)
        {
          Serial.println("Remote left");
            bleKeyboard.write(remoteLeftButton);
            delay(300);
        }
    }
    else
    {
        digitalWrite(led_blue_gpio, LOW);
    }

    Taste = Tastenfeld.getKey(); 
    if (Taste)
    {
        if (Taste == '*')
        {
            Taste = Tastenfeld.waitForKey();

            switch(Taste)
            {
                case '1':
                    ChangePageMode();
                    buttonMode = 1;
                    break;
                case '2':
                    MusicMode();
                    buttonMode = 2;
                    break;
            }
        }  
        else if (Taste == '#')
        {
            ShowMode();
        }
        else
        {
            Serial.println(Taste); 
            bleKeyboard.write(Taste);
            delay(300);
        }
    }
}
