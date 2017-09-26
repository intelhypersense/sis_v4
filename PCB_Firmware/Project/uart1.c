#include "stm8s_uart1.h"
#include "uart1.h"


#define PUTCHAR_PROTOTYPE int putchar (int c)
#define GETCHAR_PROTOTYPE int getchar (void)


u8 USART_RX_Put=0;
u8 USART_RX_Get=0;
u8 USART_RX_Buff[USART_RX_BUFF_LEN]={0};

void uart1_init(void)
{
  CLK_HSIPrescalerConfig(CLK_PRESCALER_HSIDIV1);
    
  UART1_DeInit();
  /* UART1 configuration ------------------------------------------------------*/
  /* UART1 configured as follow:
        - BaudRate = 115200 baud  
        - Word Length = 8 Bits
        - One Stop Bit
        - No parity
        - Receive and transmit enabled
        - UART1 Clock disabled
  */
  UART1_Init((uint32_t)115200, UART1_WORDLENGTH_8D, UART1_STOPBITS_1, UART1_PARITY_NO,
              UART1_SYNCMODE_CLOCK_DISABLE, UART1_MODE_TXRX_ENABLE);
  
/* Enable UART1 Receive interrupt*/
    UART1_ITConfig(UART1_IT_RXNE, ENABLE);
}

/**
  * @brief Retargets the C library printf function to the UART.
  * @param c Character to send
  * @retval char Character sent
  */
PUTCHAR_PROTOTYPE
{
  /* Write a character to the UART1 */
  UART1_SendData8(c);
  /* Loop until the end of transmission */
  while (UART1_GetFlagStatus(UART1_FLAG_TXE) == RESET);

  return (c);
}

/**
  * @brief Retargets the C library scanf function to the USART.
  * @param None
  * @retval char Character to Read
  */
GETCHAR_PROTOTYPE
{
#ifdef _COSMIC_
  char c = 0;
#else
  int c = 0;
#endif
  /* Loop until the Read data register flag is SET */
  while (UART1_GetFlagStatus(UART1_FLAG_RXNE) == RESET);
    c = UART1_ReceiveData8();
  return (c);
}

unsigned char usart_data_recv(unsigned char *return_value)
{
    unsigned char usart_rec_data,usart_rec_data_num;
    static u16 usart_rec_byte_count;
    /*-------------Print the received data,just for test--------------*/
    if(USART_RX_Put!=USART_RX_Get)
    {
        usart_rec_byte_count=USART_RX_Put;
        DelayXms(2);
        //DelayXms(5);
        if(usart_rec_byte_count==USART_RX_Put)//delay time out,receive no data in 2 ms , finished
        {
            usart_rec_data_num=0;
            while(USART_RX_Put!=USART_RX_Get)//There are data to be received
            {
                usart_rec_data=USART_RX_Buff[USART_RX_Get];
                USART_RX_Get++;
                if(USART_RX_Get>= USART_RX_BUFF_LEN)
                        USART_RX_Get=0;
                //printf("%c",usart_rec_data);
                *(return_value+usart_rec_data_num)=usart_rec_data;
                usart_rec_data_num++;
            }
            //return 1;
            return usart_rec_data_num;
        }
    }
    return 0;
}

/**************************************
Function:		DelayXus
Description:	
***************************************/
void DelayXus(u16 cnt)
{
	while(cnt--)
	{
          nop();
          nop();
          nop();
          nop();
	}
}
/**************************************
Function:		DelayXms
Description:	
***************************************/
void DelayXms(u8 cnt)
{
	u16 i;
  
  	while(cnt--)
  	{
          for(i=0;i<1000;i++)
          {
            nop();
            nop();
            nop();
            nop();
          }
  	}
}

void send_char_USART(u8 ch) 
{
    //USART_SendData(USART1,(uint8_t) ch);
    //while (USART_GetFlagStatus(USART1, USART_FLAG_TC) == RESET);
    while((UART1->SR&0X40)==0);  
    UART1->DR = (u8) ch;      
}


void send1_string_USART( u8 *str, u16 strlen)
{
	u16 k= 0 ;
	
	do { send_char_USART(*(str + k)); k++; }
	while (k < strlen);
	
}


void send_string_USART( u8 *str)
{
	u16 k= 0 ;
	while(*(str + k)!='\0')
	{
		send_char_USART(*(str + k));
		k++;
	}
}

/**
  * @brief  UART1 RX Interrupt routine
  * @param None
  * @retval
  * None
  */
 INTERRUPT_HANDLER(UART1_RX_IRQHandler, 18)
{
  /* In order to detect unexpected events during development,
     it is recommended to set a breakpoint on the following instruction.
  */
  USART_RX_Buff[USART_RX_Put] = UART1->DR;
  USART_RX_Put++;
  if(USART_RX_Put >= USART_RX_BUFF_LEN)
  {
     USART_RX_Put = 0;
  }
  
}