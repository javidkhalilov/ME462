#include "main.h"

#include <stdlibm.h>
#include <MechaBoardSettings.h>
#include <MechaBoardUSB.h>

#byte    PORTB = 0xF81

#define LED_MASK(x)   ( 0x00 + (x>>2 & 0x20) + (x>>3 & 0x0E) )  

#DEFINE USB_HID_DEVICE     FALSE
#define USB_EP1_TX_ENABLE  USB_ENABLE_BULK  //turn on EP1 for IN bulk/interrupt transfers
#define USB_EP1_RX_ENABLE  USB_ENABLE_BULK  //turn on EP1 for OUT bulk/interrupt transfers
#define EXP_SIZE  64
#define USB_EP1_TX_SIZE    EXP_SIZE  //size to allocate for the tx endpoint 1 buffer TX is to the computer
#define USB_EP1_RX_SIZE    EXP_SIZE   //size to allocate for the rx endpoint 1 buffer RX is from the computer

#include <pic18_usb.h>     //required to use USB Bulk mode
#include <usb_desc_bulk.h> //USB Configuration and Device descriptors for this USB device
#include <usb.c>           //handles usb setup tokens and get descriptor reports
//#include <usb_desc_mouse.h>

#define DATA_LENGTH 64     //length of the data get

int8 a,b,c,d,i,j;
//int8 x;

int8* data;
int8* sdata;

#int_ext
void ext_kesme()
{
j = 1;
}

void one_step_forward()
{

output_high(pin_b2);
delay_ms(10);
output_low(pin_b2);
delay_ms(10);
}

void one_step_backward()
{

output_high(pin_b2);
delay_ms(20);
output_low(pin_b2);
delay_ms(20);
}

void finished()
{
delay_ms(10);
sdata[1] = 1;
usb_puts(1,sdata,25,0);
delay_ms(10);
}

void measure()
{
sdata[2] = 1;
usb_puts(1,sdata,25,0);


}




void main()
{
int16 st,x;
data=(int8*) malloc(DATA_LENGTH);
sdata=(int8*) malloc(DATA_LENGTH);
   
   setup_adc_ports(AN0|VSS_VDD);
   setup_adc(ADC_CLOCK_INTERNAL);
   setup_psp(PSP_DISABLED);
   setup_spi(SPI_SS_DISABLED);
   setup_wdt(WDT_OFF);
   setup_timer_0(RTCC_INTERNAL);
   setup_timer_1(T1_DISABLED);
   setup_timer_2(T2_DISABLED,0,1);
   setup_timer_3(T3_DISABLED|T3_DIV_BY_1);
   setup_comparator(NC_NC_NC_NC);
   setup_vref(FALSE);
//Setup_Oscillator parameter not selected from Intr Oscillotar Config tab

   // TODO: USER CODE!!
   
   ext_int_edge(H_TO_L);
   enable_interrupts(INT_EXT);
   enable_interrupts(global);
   
   
   usb_init();
   
   while(true)
   {       
      if(usb_enumerated())
      {
       again:  
            usb_puts(1,sdata,25,0);    
            output_high((led3));

            if ( usb_kbhit(1) )
            {          
            usb_get_packet(1,data,8);
                     a = data[1];
                     b = data[2];
                     c = data[3];
                     d = data[4]; 
                     i = data[5];
                     
                     if(d == 1)
                     {
                     output_high(pin_b3);
 /*                    st=83*a;
                     for (x = 0; x < st;x++) one_step_forward();                   

                     delay_ms(1000);
                     st = 83*c;
                     
                     for (j = 0; j <= i; j++)
                     {
                        for (x = 0; x < st; x++) 
                        {
                           one_step_forward();
                        }
                        delay_ms(2000);
                        measure();
                     }
                     */
                     }  
                     
                     else if (d == 2)
                     {
                     j = 0;
                     while(1)
                     {
                     one_step_backward();
                     if (j == 1) break;
                     
                     }
                     }
                     else if (d == 3)
                     {
                     one_step_backward();
                     }

                     finished();
            }
            
            goto again;
   }
   }
}


