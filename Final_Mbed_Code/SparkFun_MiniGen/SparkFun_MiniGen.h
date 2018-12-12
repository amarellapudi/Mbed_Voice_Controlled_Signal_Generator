/****************************************************************
Core header file for MiniGen board support

This code is beerware; if you use it, please buy me (or any other
SparkFun employee) a cold beverage next time you run into one of
us at the local.

2 Jan 2014- Mike Hord, SparkFun Electronics

Code developed in Arduino 1.0.5, on an Arduino Pro Mini 5V.

**Updated to Arduino 1.6.4 5/2015**

Edited by Aniruddh Marellapudi to be compatible
with mbed LPC1768
****************************************************************/

#ifndef SparkFun_MiniGen_h
#define SparkFun_MiniGen_h

#include <mbed.h>

class MiniGen
{
  private:
  void configSPIPeripheral();
  
  public:
  enum MODE {TRI, SINE, SQUARE, SQUARE_2};
  enum FREQREG {FREQ0, FREQ1};
  enum PHASEREG {PHASE0, PHASE1};
  enum FREQADJUSTMODE {FULL, COARSE, FINE};
  
  MiniGen(PinName mosi, PinName miso, PinName sclk, PinName cs);
  
  void SPIWrite(uint16_t data);
  void reset();
  void setMode(MODE newMode);
  void selectFreqReg(FREQREG reg);
  void selectPhaseReg(PHASEREG reg);
  void setFreqAdjustMode(FREQADJUSTMODE newMode);
  void adjustPhaseShift(PHASEREG reg, uint16_t newPhase);
  void adjustFreq(FREQREG reg, FREQADJUSTMODE mode, uint32_t newFreq);
  void adjustFreq(FREQREG reg, FREQADJUSTMODE mode, uint16_t newFreq);
  void adjustFreq(FREQREG reg, uint32_t newFreq);
  void adjustFreq(FREQREG reg, uint16_t newFreq);
  uint32_t freqCalc(float desiredFrequency);
  
  SPI _spi;
  DigitalOut _cs;
  uint16_t configReg;
};

#endif