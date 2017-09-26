#ifndef __UART1_HEADER_H
#define __UART1_HEADER_H

#include "stdio.h"

#define USART_RX_BUFF_LEN 250

#define OPEN_UART1_PRINT 0

#define ADD_TEST_FUNCTION 0

#define PROCESS_LONG_CMD  1

#define TIM4_CONTROL      1

#define USE_SET_COLOUR    1


#if ADD_TEST_FUNCTION
  #define PLAY_ONE_BY_ONE     "AT+ONE"
  #define PLAY_ALL_THE_SAME   "AT+ALL"
  #define STOP_TEST           "AT+STOP"
  #define TEST_ALL_DELAY      1000

  #define PLAY_ID_01          "PLAYID=01"
  #define PLAY_ID_02          "PLAYID=02"
  #define PLAY_ID_03          "PLAYID=03"
  #define PLAY_ID_04          "PLAYID=04"
  #define PLAY_ID_05          "PLAYID=05"
  #define PLAY_ID_06          "PLAYID=06"
  #define PLAY_ID_07          "PLAYID=07"
#endif

extern u8 USART_RX_Put;
extern u8 USART_RX_Get;
extern u8 USART_RX_Buff[USART_RX_BUFF_LEN];

void uart1_init(void);

unsigned char usart_data_recv(unsigned char *return_value);

void DelayXus(u16 cnt);
void DelayXms(u8 cnt);
void send_char_USART(u8 ch) ;
void send1_string_USART( u8 *str, u16 strlen);
void send_string_USART( u8 *str);
#endif