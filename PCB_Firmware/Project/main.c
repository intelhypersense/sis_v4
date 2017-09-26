/**
  ******************************************************************************
  * @file    Project/main.c 
  * @author  MCD Application Team
  * @version V2.2.0
  * @date    30-September-2014
  * @brief   Main program body
   ******************************************************************************
  * @attention
  *
  * <h2><center>&copy; COPYRIGHT 2014 STMicroelectronics</center></h2>
  *
  * Licensed under MCD-ST Liberty SW License Agreement V2, (the "License");
  * You may not use this file except in compliance with the License.
  * You may obtain a copy of the License at:
  *
  *        http://www.st.com/software_license_agreement_liberty_v2
  *
  * Unless required by applicable law or agreed to in writing, software 
  * distributed under the License is distributed on an "AS IS" BASIS, 
  * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  * See the License for the specific language governing permissions and
  * limitations under the License.
  *
  ******************************************************************************
  */ 

/* Includes ------------------------------------------------------------------*/
#include "stm8s.h"
#include "uart1.h"
#include "motor.h"   
#include "WS2812B.h"  
#include <string.h>
    
/* Private defines -----------------------------------------------------------*/
#define PROCESS_DIFF_CMD 1
#define TIM4_PERIOD 124
#define SET_ID        "SETID="
#define PLAY_ID       "PLAYID="

#if PROCESS_DIFF_CMD
  //#define PLAY_ON_OFF       "P+O"
  #define PLAY_ON_OFF       "PO"//数据头至少要用两个，因为力度值那个字节可能会出现等于‘P’（0x50=80）的情况，只用P会影响底层对于指令的处理！！！
#endif

#define PLAY_ON       "PLAYON="
#define PLAY_OFF      "PLAYOFF="


#define TEST_COLUUR   "COLOR+"
    
#define PLAY_TIME      100

#define SAVE_ID_ADDR     0x4008
#define SAVE_COLOR_ADRR  0x4018
#define SAVE_PWM_ADRR    0x4013

#define ON       1
#define OFF      0

#define UART_REC_DATA_LEN 240
#define PROGRAM_DEV_ID    0x1C
    
/* Private function prototypes -----------------------------------------------*/
static void TIM4_Config(void);
static void play_on_or_off(u8 play_id,u8 state,u32 play_color);
static void play_auto_off();
static void TIM1_init(u16 n_ms);

#if PROCESS_DIFF_CMD
void uart_data_process(u8 *rec_data,u8 rec_data_num);
#endif

#if PROCESS_LONG_CMD
static u8 Find_My_PLAY_On_ID(u8 *cmd_str,u8 cmd_num,u8 my_id);
static u8 Find_My_Off_ID(u8 *cmd_str,u8 cmd_num,u8 my_id);
static u32 HexStrToDec(u8 *str, int len);
#endif

#if ADD_TEST_FUNCTION
  static void Test_All(u8 step);
#endif
  
#if USE_SET_COLOUR
u32 read_saved_color(u8 saved_colour);
#endif

u8 pwm_duty_value;
  
/* Private functions ---------------------------------------------------------*/
u8 USART_Receive_Data[UART_REC_DATA_LEN];
u8 USART_Receive_Data_Count; 
u8 led_color='B';
u16 TimingDelay;
u8 set_device_id;
u8 play_device_id;
u8 read_device_id;
u8 pwm_duty_cycle=50;//默认值
u32 test_color_rgb_val;

#if USE_SET_COLOUR
u32 set_color_for_play=BLUE;//默认play颜色：蓝色
u8  saved_colour_byte;
#endif

#if ADD_TEST_FUNCTION
u16 delay_ms_count=0;
u8 test_all_step=0;
u8 test_all_step_old=0;
#endif

volatile u8 fPowerOn_flag = FALSE;//TRUE;//FALSE;

void main(void)
{
  CLK_DeInit();//HSI on,and it is the master clock by default
  CLK_SYSCLKConfig(CLK_PRESCALER_HSIDIV1);//HSI prescaler is 1,so the master clock is 16M
  CLK_SYSCLKConfig(CLK_PRESCALER_CPUDIV1);//CPU clock division factors is 1,so the CPU clock is 16M;
  uart1_init();
  ws2812_init();
  Led_Init();
  Motor_Init();
  //enableInterrupts();
  DelayXms(250);		////To ensure the led is off,delay 250ms
  set_led_colour(BLACK);//To ensure the led is off
  
#if !TIM4_CONTROL
  TIM4_Config();
#endif
  
  /***********Disable the unsed peripheral clock************/
  //CLK_PeripheralClockConfig(CLK_PERIPHERAL_TIMER1,DISABLE);
  CLK_PeripheralClockConfig(CLK_PERIPHERAL_I2C,DISABLE);
  CLK_PeripheralClockConfig(CLK_PERIPHERAL_SPI,DISABLE);
  CLK_PeripheralClockConfig(CLK_PERIPHERAL_ADC,DISABLE);
  CLK_PeripheralClockConfig(CLK_PERIPHERAL_AWU,DISABLE);
  
  read_device_id= FLASH_ReadByte(SAVE_ID_ADDR);   // read ID byte from a specific address
  
  #if USE_SET_COLOUR
  saved_colour_byte=FLASH_ReadByte(SAVE_COLOR_ADRR);  // read color byte from a specific address;
  if(saved_colour_byte==0||saved_colour_byte==0xff)
    set_color_for_play=BLUE;
  else
  {
    set_color_for_play=read_saved_color(saved_colour_byte);
  }
  #endif
  
  //TIM1_init(3000);
  
  pwm_duty_cycle=FLASH_ReadByte(SAVE_PWM_ADRR);  // read pwm duty cycle from a specific address;
  if(pwm_duty_cycle==0||pwm_duty_cycle>=100)
    pwm_duty_cycle=50;
  
  DelayXms(250);		////To ensure the led is off,delay 250ms
  /**************led white for 1 second to indicate reset****************/
  set_led_colour(WHITE);
  DelayXms(250);		//delay 250ms
  DelayXms(250);		//delay 250ms
  DelayXms(250);		//delay 250ms
  DelayXms(250);		//delay 250ms
  set_led_colour(BLACK);
  
  
#if 0
  if(read_device_id!=PROGRAM_DEV_ID)
  {
    FLASH_Unlock(FLASH_MEMTYPE_DATA);
    while((FLASH->IAPSR & FLASH_IAPSR_DUL) == 0);//wait until DATA EEPROM is unlock
    FLASH_EraseByte(SAVE_ID_ADDR);  // erase a byte in the specific address
    FLASH_ProgramByte(SAVE_ID_ADDR,PROGRAM_DEV_ID); //write a byte to a specific address in EEPROM
    while((FLASH->IAPSR & 0x04) != 0x00);   //wait until EOP=1,the end of programing EEPROM 
  }
#endif
  
  enableInterrupts();
  #if OPEN_UART1_PRINT
  printf("HW init finish\r\n");
  #endif
  /* Infinite loop */
  while (1)
  {
    if(fPowerOn_flag == FALSE)
    {
      #if 0
        TIM4_DeInit();
        TIM2_DeInit();
        TIM1_DeInit();
      
        CLK_PeripheralClockConfig(CLK_PERIPHERAL_TIMER1,DISABLE);
        CLK_PeripheralClockConfig(CLK_PERIPHERAL_TIMER2,DISABLE);
        CLK_PeripheralClockConfig(CLK_PERIPHERAL_TIMER4,DISABLE);
      #else
        TIM1->CR1 &= (uint8_t)(~0x01);
        TIM4->CR1 &= (uint8_t)(~0x01);
        TIM2->CR1 &= (uint8_t)(~0x01);

        CLK->PCKENR1 &=0x4f;//CLK_PERIPHERAL_TIMER1,CLK_PERIPHERAL_TIMER2,CLK_PERIPHERAL_TIMER4,DISABLE
        
      #endif
      wfi();
      fPowerOn_flag=TRUE; //必须要，不然唤不醒
      //halt();//Enter halt mode(low power)
    }
    /**************UART1 Test***************/
    USART_Receive_Data_Count=usart_data_recv(USART_Receive_Data);
    if(USART_Receive_Data_Count>UART_REC_DATA_LEN)
      USART_Receive_Data_Count=UART_REC_DATA_LEN;
    if(USART_Receive_Data_Count)
    {
      //*******************Test uart1******************//
#if 0
      send1_string_USART(USART_Receive_Data,USART_Receive_Data_Count);	
      //send_char_USART(0x0d);
      //send_char_USART(0x0a);
#endif
      //*******************Test motor******************//
#if !OPEN_UART1_PRINT
      if(!memcmp(USART_Receive_Data,MOTOR_ON , sizeof(MOTOR_ON)-1 )) //Motor On
      {
        if((read_device_id!=0)&&(read_device_id!=0xff))
        {
          #if 0
          pwm_duty_cycle=(8-read_device_id)*10+10;//1:80,2:70......7:20   7音最高，震动最强，低电平时间最长
          Motor_On(pwm_duty_cycle);
          #else
          pwm_duty_cycle=10*(USART_Receive_Data[5]-'0')+USART_Receive_Data[6]-'0';
          FLASH_Unlock(FLASH_MEMTYPE_DATA);
          while((FLASH->IAPSR & FLASH_IAPSR_DUL) == 0);//wait until DATA EEPROM is unlock
          FLASH_EraseByte(SAVE_PWM_ADRR);  // erase a byte in the specific address
          FLASH_ProgramByte(SAVE_PWM_ADRR,pwm_duty_cycle); //write a byte to a specific address in EEPROM
          while((FLASH->IAPSR & 0x04) != 0x00);   //wait until EOP=1,the end of programing EEPROM
          //Motor_On(pwm_duty_cycle);
          #endif
        }
        else
        {
          Motor_On(50);
        }
        
        #if OPEN_UART1_PRINT
        printf("Motor On!\r\n");
        #endif
      }
      else if(!memcmp(USART_Receive_Data,MOTOR_OFF , sizeof(MOTOR_OFF)-1 )) //Motor Off
      {
        Motor_Off();
        fPowerOn_flag=FALSE;
        #if OPEN_UART1_PRINT
        printf("Motor Off!\r\n");
        #endif
      }
      //*******************Test led******************//

      else if(!memcmp(USART_Receive_Data,SET_LED_COLOUR , sizeof(SET_LED_COLOUR)-1 )) //AT+LED
#else
      if(!memcmp(USART_Receive_Data,SET_LED_COLOUR , sizeof(SET_LED_COLOUR)-1 )) //AT+LED
#endif
      {
        if(USART_Receive_Data[6]!='D')//防止最后存的时候‘D’，导致上电后play颜色为黑色
          led_color=USART_Receive_Data[6];
        /****************save colour byte in the stm8 eeprom*****************/
        FLASH_Unlock(FLASH_MEMTYPE_DATA);
        while((FLASH->IAPSR & FLASH_IAPSR_DUL) == 0);//wait until DATA EEPROM is unlock
        FLASH_EraseByte(SAVE_COLOR_ADRR);  // erase a byte in the specific address
        FLASH_ProgramByte(SAVE_COLOR_ADRR,led_color); //write a byte to a specific address in EEPROM
        while((FLASH->IAPSR & 0x04) != 0x00);   //wait until EOP=1,the end of programing EEPROM 
        
        set_color_for_play=read_saved_color(led_color);
        
        set_color(USART_Receive_Data[6]);
        if(led_color=='D')
          fPowerOn_flag=FALSE;
      }
      //*******************Main control******************//
      else if(!memcmp(USART_Receive_Data,SET_ID , sizeof(SET_ID)-1 )) //SET_ID  SETID=xx
      {
        //set_device_id=10*(USART_Receive_Data[6]-'0')+USART_Receive_Data[7]-'0';
        set_device_id=HexStrToDec(&(USART_Receive_Data[6]),2);
        #if OPEN_UART1_PRINT
        printf("Set ID:%x!\r\n",set_device_id);
        #endif
        /****************save id byte in the stm8 eeprom*****************/
        FLASH_Unlock(FLASH_MEMTYPE_DATA);
        while((FLASH->IAPSR & FLASH_IAPSR_DUL) == 0);//wait until DATA EEPROM is unlock
        FLASH_EraseByte(SAVE_ID_ADDR);  // erase a byte in the specific address
        FLASH_ProgramByte(SAVE_ID_ADDR,set_device_id); //write a byte to a specific address in EEPROM
        while((FLASH->IAPSR & 0x04) != 0x00);   //wait until EOP=1,the end of programing EEPROM 
        
        read_device_id= FLASH_ReadByte(SAVE_ID_ADDR);   // read ID byte from a specific address
      }
      else if(!memcmp(USART_Receive_Data,PLAY_ID , sizeof(PLAY_ID)-1 )) //PLAY_ID  PLAYID=xx
      {
        #if PROCESS_LONG_CMD
          if(Find_My_PLAY_On_ID(USART_Receive_Data,USART_Receive_Data_Count,read_device_id))//
          {
            TimingDelay=PLAY_TIME;
            play_on_or_off(read_device_id,ON,set_color_for_play);//因为这里用的是read_device_id，只在初始化的时候赋值，所以设置ID后发送play指令不会立即执行
            #if TIM4_CONTROL
              TIM4_Config();
            #endif
          }
        #else
          play_device_id=10*(USART_Receive_Data[7]-'0')+USART_Receive_Data[8]-'0';
            #if OPEN_UART1_PRINT
            printf("Play ID:%d!\r\n",play_device_id);
            #endif
          
          TimingDelay=PLAY_TIME;
          play_on_or_off(play_device_id,ON,set_color_for_play);
          #if TIM4_CONTROL
              TIM4_Config();
          #endif
        #endif
      }
      else if(!memcmp(USART_Receive_Data,PLAY_ON , sizeof(PLAY_ON)-1 )) //PLAY_ON PLAYON=xx
      {
        #if PROCESS_LONG_CMD
          if(Find_My_PLAY_On_ID(USART_Receive_Data,USART_Receive_Data_Count,read_device_id))
          {
            TimingDelay=0;
            play_on_or_off(read_device_id,ON,set_color_for_play);
          }  
        #else
          play_device_id=10*(USART_Receive_Data[7]-'0')+USART_Receive_Data[8]-'0';
            #if OPEN_UART1_PRINT
            printf("Play on ID!\r\n");
            #endif
          TimingDelay=0;
          play_on_or_off(play_device_id,ON,set_color_for_play);
        #endif
        
      }
      else if(!memcmp(USART_Receive_Data,PLAY_OFF , sizeof(PLAY_OFF)-1 )) //PLAY_OFF PLAYOFF=xx
      {
        #if PROCESS_LONG_CMD
          if(Find_My_Off_ID(USART_Receive_Data,USART_Receive_Data_Count,read_device_id))
          {
            play_on_or_off(read_device_id,OFF,BLACK);
          }
        #else
          play_device_id=10*(USART_Receive_Data[8]-'0')+USART_Receive_Data[9]-'0';
            #if OPEN_UART1_PRINT
            printf("Play off ID:%d!\r\n",play_device_id);
            #endif
          play_on_or_off(play_device_id,OFF,BLACK);
          //TimingDelay=5;//stop play 5ms later ,then enter wait cmd state
        #endif
        
        
      }
      else if(!memcmp(USART_Receive_Data,TEST_COLUUR , sizeof(TEST_COLUUR)-1 )) //
      {
        test_color_rgb_val=HexStrToDec((USART_Receive_Data+sizeof(TEST_COLUUR)-1),6);
        set_led_colour(test_color_rgb_val);
      }
      
      #if PROCESS_DIFF_CMD
      uart_data_process(USART_Receive_Data,USART_Receive_Data_Count);
      #endif
      
      #if ADD_TEST_FUNCTION
      else if(!memcmp(USART_Receive_Data,PLAY_ALL_THE_SAME , sizeof(PLAY_ALL_THE_SAME)-1 )) //
      {
        fPowerOn_flag=TRUE;
        test_all_step=1;
        TimingDelay=PLAY_TIME;
        delay_ms_count=TEST_ALL_DELAY;
        #if TIM4_CONTROL
           TIM4_Config();
        #endif
      }
      else if(!memcmp(USART_Receive_Data,STOP_TEST , sizeof(STOP_TEST)-1 )) //
      {
        fPowerOn_flag=FALSE;
        test_all_step=0;
        test_all_step_old=0;
        delay_ms_count=0;
        play_on_or_off(read_device_id,OFF,BLACK);
      }
      #endif
    }
    
    #if ADD_TEST_FUNCTION
      Test_All(test_all_step);
    #endif
  }
  
}

static u32 HexStrToDec(u8 *str, int len)  
{  
    u32 tmpResult = 0;  
    // 32位机  
    if (len > 8)  
    {  
        return 0;  
    }  
  
    while (len-- > 0)  
    {  
        tmpResult <<= 4;  
        if (*str >= '0' && *str <= '9')  
        {  
            tmpResult |= *str - '0';  
        }  
        else if(*str >= 'A' && *str <= 'F')  
        {  
            tmpResult |= *str - 'A' + 10;  
        }  
        else if(*str >= 'a' && *str <= 'f')  
        {  
            tmpResult |= *str - 'a' + 10;  
        }  
        str++;  
    }  
  
    return tmpResult;  
}   

#if USE_SET_COLOUR
u32 read_saved_color(u8 saved_colour)
{
  //u32 temp_colour;
  static u32 temp_colour;//设置颜色后要关掉颜色会发送AT+LEDD,所以要用静态变量保存上一次的颜色值
  switch(saved_colour)
  {
    case 'R':
      temp_colour=RED;
      break;
    case 'G':
      temp_colour=GREEN;
      break;
    case 'B':
      temp_colour=BLUE;
      break;
    case 'Y':
      temp_colour=YELLOW;
      break;
    case 'W':
      temp_colour=WHITE;
      break;
    case 'P':
      temp_colour=PURPLE;
      break;
    default://case 'D':
      break;
  }
  return temp_colour;
}

#endif


#if ADD_TEST_FUNCTION
static void Test_All(u8 all_step)
{
  if((all_step>0)&&(test_all_step_old!=all_step))
    {
      test_all_step_old=all_step;
      switch(all_step)
      {
        case 1:
          play_on_or_off(read_device_id,ON,WHITE);
          break;
        case 2:
          play_on_or_off(read_device_id,ON,RED);
          break;
        case 3:
          play_on_or_off(read_device_id,ON,YELLOW);
          break;
        case 4:
          play_on_or_off(read_device_id,ON,BLUE);
          break;
        case 5:
          play_on_or_off(read_device_id,ON,GREEN);
          break;
        case 6:
          play_on_or_off(read_device_id,ON,PURPLE);
          break; 
        default:
          break;
      }
    }
}
#endif

#if PROCESS_LONG_CMD
static u8 Find_My_PLAY_On_ID(u8 *cmd_str,u8 cmd_num,u8 my_id)//每个设备去比较，看有没有自己的ID，有的话就执行play
{
  u8 i,temp_id;
  for(i=0;i<cmd_num;i+=9)
  {
    //temp_id=10*(cmd_str[7+i]-'0')+cmd_str[8+i]-'0';
    temp_id=HexStrToDec(&(cmd_str[7+i]),2);
    if(temp_id==my_id)
    {
      return 1;
    }
  }
  return 0;
}

static u8 Find_My_Off_ID(u8 *cmd_str,u8 cmd_num,u8 my_id)
{
  u8 i,temp_id;
  for(i=0;i<cmd_num;i+=10)
  {
    //temp_id=10*(cmd_str[8+i]-'0')+cmd_str[9+i]-'0';
    temp_id=HexStrToDec(&(cmd_str[8+i]),2);
    if(temp_id==my_id)
    {
      return 1;
    }
  }
  return 0;
}
#endif

static u32 get_color_on_id(u8 play_id)
{
  u32 color_val_on_id;
  play_id=play_id%10;//限制在1-8
  switch(play_id)
  {
    case 1:
      color_val_on_id=RED;
      break;
    case 2:
      color_val_on_id=ORANGE;
      break;
    case 3:
      color_val_on_id=YELLOW;
      break;
    case 4:
      color_val_on_id=GREEN;
      break;
    case 5:
      color_val_on_id=CYAN;
      break;
    case 6:
      color_val_on_id=BLUE;
      break;
    case 7:
      color_val_on_id=PURPLE;
      break;
    default:
      color_val_on_id=WHITE;
      break;
  }
  return color_val_on_id;
}

static void play_on_or_off(u8 play_id,u8 state,u32 play_color)
{
  //u8 pwm_duty_cycle;
  //u8 read_device_id;
  //read_device_id= FLASH_ReadByte(SAVE_ID_ADDR);   // read a byte from a specific address
  #if OPEN_UART1_PRINT
  printf("Read ID:%x!\r\n",read_device_id);
  #endif
  if(play_id==read_device_id)
  {
    if(state==ON)
    {
      //pwm_duty_cycle=(9-play_id)*10+10;//1:90,2:80......8:20   7音最高，震动最强，低电平时间最长
      //Motor_On(pwm_duty_cycle);
      Motor_On(pwm_duty_value);
      //set_led_colour(play_color);
      //set_led_colour(get_color_on_id(play_id));
      fPowerOn_flag=TRUE;
      TIM1_init(3000);
    }
    else
    {
      fPowerOn_flag=FALSE;
      Motor_Off();
      //set_led_colour(BLACK);
      //TIM1_Cmd(DISABLE);
      TIM1->CR1 &= (uint8_t)(~0x01);
    }
  }
}



static void play_auto_off()
{ 
  #if ADD_TEST_FUNCTION
  if(test_all_step==0)//没有进行所有的同时动作时
    fPowerOn_flag=FALSE;
  #else
  fPowerOn_flag=FALSE;
  #endif
  Motor_Off();
  set_led_colour(BLACK);
}

static void TIM4_Config(void)
{
  /* TIM4 configuration:
   - TIM4CLK is set to 16 MHz, the TIM4 Prescaler is equal to 128 so the TIM1 counter
   clock used is 16 MHz / 128 = 125 000 Hz
  - With 125 000 Hz we can generate time base:
      max time base is 2.048 ms if TIM4_PERIOD = 255 --> (255 + 1) / 125000 = 2.048 ms
      min time base is 0.016 ms if TIM4_PERIOD = 1   --> (  1 + 1) / 125000 = 0.016 ms
  - In this example we need to generate a time base equal to 1 ms
   so TIM4_PERIOD = (0.001 * 125000 - 1) = 124 */

  //CLK_PeripheralClockConfig(CLK_PERIPHERAL_TIMER4,ENABLE);
  CLK->PCKENR1 |= 0x10;
  /* Time base configuration */
  TIM4_TimeBaseInit(TIM4_PRESCALER_128, TIM4_PERIOD);
  /* Clear TIM4 update flag */
  TIM4_ClearFlag(TIM4_FLAG_UPDATE);
  /* Enable update interrupt */
  TIM4_ITConfig(TIM4_IT_UPDATE, ENABLE);
  
  /* enable interrupts */
  //enableInterrupts();

  /* Enable TIM4 */
  TIM4_Cmd(ENABLE);
}


void TimingDelay_Decrement(void)
{
  if (TimingDelay != 0x00)
  {
    TimingDelay--;
    if(TimingDelay==0)
    {
      play_auto_off();
    }
  }
}



 INTERRUPT_HANDLER(TIM4_UPD_OVF_IRQHandler, 23)
{
  
   TimingDelay_Decrement();
 #if ADD_TEST_FUNCTION
   if (delay_ms_count != 0x00)
   {
    delay_ms_count--;
    if(delay_ms_count==0)
    {
      test_all_step++;
      if(test_all_step>6)//共六种颜色
      {
        test_all_step=1;
      }
      TimingDelay=PLAY_TIME;
      delay_ms_count=TEST_ALL_DELAY;
    }
   }
  #endif
  /* Cleat Interrupt Pending bit */
  TIM4_ClearITPendingBit(TIM4_IT_UPDATE);

}

INTERRUPT_HANDLER(TIM2_UPD_OVF_BRK_IRQHandler, 13)
 {
  /* In order to detect unexpected events during development,
     it is recommended to set a breakpoint on the following instruction.
  */
 }
 
 INTERRUPT_HANDLER(TIM1_UPD_OVF_TRG_BRK_IRQHandler, 11)
{
  /* In order to detect unexpected events during development,
     it is recommended to set a breakpoint on the following instruction.
  */
  //play_auto_off();
  TIM1->SR1&=~0x01;//清除中断标志
  play_auto_off();
}

#if PROCESS_DIFF_CMD
void uart_data_process(u8 *rec_data,u8 rec_data_num)
{
  u8 data_count,get_id,on_off_state;
  float play_color_val_temp;
  u32 play_color_val;
  data_count=0;
  while(data_count<rec_data_num)
  {
    if(!memcmp((rec_data+data_count),PLAY_ON_OFF , sizeof(PLAY_ON_OFF)-1 ))
    {
      if(*(rec_data+data_count+2)=='N')//ON
        on_off_state=1;
      else//if(*(rec_data+data_count+2)=='F')//ON
        on_off_state=0;
      get_id=HexStrToDec((rec_data+data_count+3),2);
      
      pwm_duty_value=*(rec_data+data_count+5);
      play_color_val_temp=pwm_duty_value;//保存收到的力度值
      if(pwm_duty_value>=100)
      {
        pwm_duty_value=99;
        play_color_val_temp=100;
      }

      #if 1
      
      pwm_duty_value=100-pwm_duty_value;
      TimingDelay=0;
      play_on_or_off(get_id,on_off_state,BLUE);//OK
      #else
      if((pwm_duty_value<20)&&(pwm_duty_value>0))//保证最小的震动强度为20（为了与灯的亮度匹配），且为0不震动
      {
        pwm_duty_value=20;
      }
      
      pwm_duty_value=100-pwm_duty_value;
      TimingDelay=0;
      
      play_color_val_temp*=2.55;//1-100 to 2.55-255
      play_color_val=(u32)play_color_val_temp;
      
      if(BLUE==set_color_for_play)
      {
        play_on_or_off(get_id,on_off_state,play_color_val);//OK
      }
      else if(RED==set_color_for_play)
      {
        play_color_val<<=16;
        play_on_or_off(get_id,on_off_state,play_color_val);
      }
       else if(GREEN==set_color_for_play)
      {
        play_color_val<<=8;
        play_on_or_off(get_id,on_off_state,play_color_val);
      }
      #endif
      
      //play_on_or_off(get_id,on_off_state,set_color_for_play);
    }
    //data_count+=6;
    data_count+=1;//可能出现串口丢包而导致后面的指令错位，数据字节移位小于6可能不会因为找不到数据头而丢掉后面所有的数据
  }
}
#endif

#if 0
static void TIM1_init(u16 n_ms)
{
  //CLK_PeripheralClockConfig(CLK_PERIPHERAL_TIMER1,ENABLE);
  CLK->PCKENR1 |= 0x80;
  TIM1->CR1 &= (uint8_t)(~0x01);
  TIM1->SR1 &= 0x00;
  TIM1->CNTRH =0x00;//重新计时
  TIM1->CNTRL =0x00;
  //TIM1->PSCRH = 0x1F; // 8M系统时钟经预分频f=fck/(PSCR+1)
  //TIM1->PSCRL = 0x3F; // PSCR=0x1F3F，f=8M/(0x1F3F+1)=1000Hz，每个计数周期1ms
  TIM1->PSCRH = 0x3E; // 16M系统时钟经预分频f=fck/(PSCR+1)
  TIM1->PSCRL = 0x7F; // PSCR=0x3E7F，f=16M/(15999+1)=1000Hz，每个计数周期1ms
  TIM1->ARRH = (u8)(n_ms >> 8); 
  TIM1->ARRL = (u8)(n_ms); 
  TIM1->IER |= 0x01; // 允许更新中断
  TIM1->CR1 |= 0x01; // 计数器使能，开始计数
}
#else
static void TIM1_init(u16 n_ms)
{
  CLK->PCKENR1 |= 0x80;
  TIM1->IER &= ~0x01; // 禁止更新中断
  TIM1->CR1 &= (uint8_t)(~0x01);
  TIM1->SR1 &= 0x00;
  TIM1->CNTRH =0x00;//重新计时
  TIM1->CNTRL =0x00;
  //TIM1->PSCRH = 0x1F; // 8M系统时钟经预分频f=fck/(PSCR+1)
  //TIM1->PSCRL = 0x3F; // PSCR=0x1F3F，f=8M/(0x1F3F+1)=1000Hz，每个计数周期1ms
  TIM1->PSCRH = 0x3E; // 16M系统时钟经预分频f=fck/(PSCR+1)
  TIM1->PSCRL = 0x7F; // PSCR=0x3E7F，f=16M/(15999+1)=1000Hz，每个计数周期1ms
  TIM1->ARRH = (u8)(3000 >> 8); 
  TIM1->ARRL = (u8)(3000); 
  TIM1->CR1 |= 0x01; // 计数器使能，开始计数
  TIM1->EGR |=0x01;
  while((TIM1->SR1&0x01)==0);
  if(TIM1->SR1)
    TIM1->SR1=0;
  TIM1->IER |= 0x01; // 允许更新中断
}
#endif
  
#ifdef USE_FULL_ASSERT

/**
  * @brief  Reports the name of the source file and the source line number
  *   where the assert_param error has occurred.
  * @param file: pointer to the source file name
  * @param line: assert_param error line source number
  * @retval : None
  */
void assert_failed(u8* file, u32 line)
{ 
  /* User can add his own implementation to report the file name and line number,
     ex: printf("Wrong parameters value: file %s on line %d\r\n", file, line) */
  #if OPEN_UART1_PRINT
  printf("assert_failed!\r\n");
  #endif
  /* Infinite loop */
  while (1)
  {
  }
}
#endif


/************************ (C) COPYRIGHT STMicroelectronics *****END OF FILE****/
