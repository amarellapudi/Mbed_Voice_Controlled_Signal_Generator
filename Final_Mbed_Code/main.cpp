#include "mbed.h"
#include "mbed_rpc.h"
#include "mbed.h"
#include "SparkFun_MiniGen.h"
#include <stdlib.h>
#include <string> 
#include "uLCD_4DGL.h"
#include <ctype.h>
#include "rpc.h"

MiniGen gen(p11, p12, p13, p8); // function generator chip (MOSI, MISO, SCK, CS)
DigitalOut led(LED1);
Serial Pi(p9,p10);
Serial pc(USBTX,USBRX);
uLCD_4DGL uLCD(p28,p27,p30);
Ticker on_indicator;
int signal_gen_MODE = -1;
float desired_frequency = 0.0;
int RPC_freq = 100;

// RPC variables
RPCVariable<int> RPC_MODE(&signal_gen_MODE, "MODE");
RPCVariable<int> RPC_FREQ(&RPC_freq, "FREQ");

void flip() { led = !led; } // simple on-indicator with LED1

int main() {
    // On indicator ticker at 500ms
	on_indicator.attach(&flip, 0.5);

    // Set up LCD and serial input from Raspberry Pi
	pc.baud(9600);
	Pi.baud(115200);
	uLCD.baudrate(3000000);
	uLCD.cls();
	uLCD.background_color(BLACK);

    // Reset Signal Generator, default behavior is 100Hz sine wave
	gen.reset();
	gen.setFreqAdjustMode(MiniGen::FULL);
	desired_frequency = 100;

    // RPC Buffers
	char buf[256], outbuf[256];
    // Accept serial input from Raspberry Pi, which changes the frequency
	char Buffer[11], fBuffer[7], type[2];
	int n = 0;
	char r;
	while(1) {
		if(Pi.readable()){	// reach command from the raspberry pi in the form "si.4000"
			r = Pi.getc();	// two-letter signal type, followed by a period, followed by the frequency
		uLCD.putc(r);
		Buffer[n] = r;
		n = n+1;
		while(r != '\n'){
			if(Pi.readable()){
				r = Pi.getc();
				uLCD.putc(r);
				Buffer[n] = r;   
				n = n+1;
			}
		}
		n = n-4;
	}
	if(n>0){
			for (int i = 0; i < n;i++){			// parse input from the raspberry pi
				fBuffer[7-n+i] = Buffer[i+3];
			}

			type[0] = Buffer[0];
			type[1] = Buffer[1];

			switch(type[1]){	// set signal generator mode
				case 'r':
				gen.setMode(gen.TRI);
				signal_gen_MODE = -3;
				break;
				case 'q':
				gen.setMode(gen.SQUARE);
				signal_gen_MODE = -2;
				break;
				case 'i':
				gen.setMode(gen.SINE);
				signal_gen_MODE = -1;
				break;
			}

			if(n != 0){
				// parse command frequency
				desired_frequency = atof(&fBuffer[7-n]);
	        	// software multiplier function to get the correct frequency
				unsigned long freqReg = gen.freqCalc(desired_frequency);
	        	// Send SPI command to adjust frequency
				gen.adjustFreq(gen.FREQ0, gen.FULL, (uint32_t)freqReg);
			}
			n = 0;
		}

		RPC_freq = int(desired_frequency);	// update RPC variable
		if (pc.readable()) {				// small while loop to communicate RPC
			while(1) {
				pc.gets(buf, 256);
				RPC::call(buf, outbuf); 
				pc.printf("%s\n", outbuf);
				if (!pc.readable()) {
					break;
				}
			}
		}

	}
}