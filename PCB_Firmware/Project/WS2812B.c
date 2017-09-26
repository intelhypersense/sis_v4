#include "WS2812B.h" 
#include "uart1.h"

#if USE_ALL_COLOR
//#define set_led_bit(x) {switch(x){case 0:LED_PORT->ODR |= LED_PIN; nop();nop();nop();nop();nop();nop();LED_PORT->ODR &= (~LED_PIN);nop();nop();nop();nop();nop();nop();break;case 1:LED_PORT->ODR |= LED_PIN; nop();nop();nop();nop();nop();nop();nop();nop();nop();nop();LED_PORT->ODR &= (~LED_PIN);nop();break;default:break;}}
#define set_led_bit(x) {if(x==0){LED_PORT->ODR |= 0x80; nop();nop();nop();nop();nop();nop();LED_PORT->ODR &= 0x7f;nop();nop();nop();nop();nop();nop();nop();}else{LED_PORT->ODR |= LED_PIN;nop(); nop();nop();nop();nop();nop();nop();nop();nop();nop();nop();LED_PORT->ODR &= (~LED_PIN);nop();nop();nop();}}
#else
  #define set_led_1() {LED_PORT->ODR |= LED_PIN; nop();nop();nop();nop();nop();nop();nop();nop();nop();nop();LED_PORT->ODR &= (~LED_PIN);nop();nop();nop();nop();nop();nop();} //H 0.75us  / L 0.4325us
  #define set_led_0() {LED_PORT->ODR |= LED_PIN; nop();nop();nop();nop();nop();nop();LED_PORT->ODR &= (~LED_PIN);nop();nop();nop();nop();nop();nop();nop();nop();nop();nop();} //H 0.4325us / L 0.75us
#endif


void ws2812_init(void)
{
  GPIO_Init(LED_PORT, LED_PIN, GPIO_MODE_OUT_PP_HIGH_FAST);	
  set_led_colour(BLACK);
}


void set_color(u8 color)
{
  switch(color)
  {
    case 'R':
      set_led_colour(RED);
      #if OPEN_UART1_PRINT
      printf("LED red!\r\n");
      #endif
      break;
    case 'G':
      set_led_colour(GREEN);
      #if OPEN_UART1_PRINT
      printf("LED green!\r\n");
      #endif
      break;
    case 'B':
      set_led_colour(BLUE);
      #if OPEN_UART1_PRINT
      printf("LED blue!\r\n");
      #endif
      break;
    case 'Y':
      set_led_colour(YELLOW);
      #if OPEN_UART1_PRINT
      printf("LED yellow!\r\n");
      #endif
      break;
//#if USE_WHITE_COLOR
#if !OPEN_UART1_PRINT   
    case 'W':
      set_led_colour(WHITE);
      #if OPEN_UART1_PRINT
      printf("LED white!\r\n");
      #endif
      break;
#endif
    case 'D'://Dack
      set_led_colour(BLACK);
      #if OPEN_UART1_PRINT
      printf("LED black!\r\n");
      #endif
      break;
    case 'P'://PURPLE
      set_led_colour(PURPLE);
      #if OPEN_UART1_PRINT
      printf("LED purple!\r\n");
      #endif
      break;
    default:
      break;
  }
}


#if USE_ALL_COLOR
volatile uint8_t grb_bit[24];
uint32_t print_test_colour;
void set_led_colour(uint32_t color)
{
  //uint32_t i;
  int i;
#if 1
  /***************先将颜色的RGB转换为与WS2812B芯片相符合的颜色排列顺序GRB*******************/
  color=((color<<8)&0xff0000)|((color>>8)&0x00ff00)|(color&0x0000ff);//GRB
#endif
  
  print_test_colour=color;
  /************先计算出来传入set_led_bit（）函数的参数值，加快速度************/
  
  
#if 0
  for(i=24;i>0;i--)
  {
    if((color&(0x000001<<(i-1)))!=0)
    {
      grb_bit[i-1]=1;
    }
    else //if((color&(0x000001<<i))==0)
    {
      grb_bit[i-1]=0; 
    }
  }
#else
  for(i=23;i>-1;i--)
  {
    if(((color>>i)&0x01)!=0)
    {
      grb_bit[i]=1;
    }
    else //if((color&(0x000001<<i))==0)
    {
      grb_bit[i]=0; 
    }
  }
  /*
  for(i=24;i>0;i--)
  {
    if(((color>>(i-1))&0x01)!=0)
    {
      grb_bit[i-1]=1;
    }
    else //if((color&(0x000001<<i))==0)
    {
      grb_bit[i-1]=0; 
    }
  }
  */
#endif
  
  asm("sim"); // 关全局中断
  //disableInterrupts();
  
  set_led_bit(grb_bit[23]);// G 最高位
  set_led_bit(grb_bit[22]);
  set_led_bit(grb_bit[21]);
  set_led_bit(grb_bit[20]);
  set_led_bit(grb_bit[19]);
  set_led_bit(grb_bit[18]);
  set_led_bit(grb_bit[17]);
  set_led_bit(grb_bit[16]);
  
  set_led_bit(grb_bit[15]);//R
  set_led_bit(grb_bit[14]);
  set_led_bit(grb_bit[13]);
  set_led_bit(grb_bit[12]);
  set_led_bit(grb_bit[11]);
  set_led_bit(grb_bit[10]);
  set_led_bit(grb_bit[9]);
  set_led_bit(grb_bit[8]);
  
  set_led_bit(grb_bit[7]);//B
  set_led_bit(grb_bit[6]);
  set_led_bit(grb_bit[5]);
  set_led_bit(grb_bit[4]);
  set_led_bit(grb_bit[3]);
  set_led_bit(grb_bit[2]);
  set_led_bit(grb_bit[1]);
  set_led_bit(grb_bit[0]);
  
  //enableInterrupts();
  asm("rim"); // 开全局中断
}
#else
void set_led_colour(uint32_t color)
{
  
	switch(color)
        {
           case RED:
            // Green
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
				
            //Red
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            
            //Blue
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            break;
          case GREEN:					
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
    
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
    
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            break;
          case BLUE:			
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
    
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
    
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            break;
        case YELLOW:
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            break;
#if !OPEN_UART1_PRINT
         case WHITE:
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            set_led_1() ;
            break;
#endif
         case BLACK:
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            set_led_0();
            break;
	default:
            break;
  }	
}
#endif
