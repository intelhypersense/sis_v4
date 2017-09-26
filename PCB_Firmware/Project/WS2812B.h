#ifndef __WS2812B_H
#define __WS2812B_H

#include "stm8s.h"

#define LED_PORT GPIOC	
#define LED_PIN  GPIO_PIN_7

/*
#define RED     0x00FF00//0xFF0000  led芯片为24bit GRB排列，而颜色对照表为RGB排列
#define GREEN   0xFF0000//0x00FF00
#define BLUE    0x0000FF
#define YELLOW  0xFFFF00
*/

/**************不用将RGB转换为GRB了，设置颜色的代码里自动转换*****************/
#define RED        0xFF0000//红色
#define ORANGE     0xFF8000//橙色
#define YELLOW     0xFFFF00//黄色
#define GREEN      0x00FF00//绿色
#define CYAN       0x00FFFF//青色
#define BLUE       0x0000FF//蓝色
#define PURPLE     0xFF00FF//紫色

#define WHITE  0xFFFFFF
#define BLACK  0x000000

#define USE_WHITE_COLOR 0
#define USE_ALL_COLOR 1
 
#define LED_RED     "AT+RED"
#define LED_GREEN   "AT+GREEN"
#define LED_BLUE    "AT+BLUE"

#define SET_LED_COLOUR    "AT+LED"

void ws2812_init(void);
void set_led_colour(uint32_t color);

#if USE_ALL_COLOR
void set_led_colour1(uint32_t color);
#endif

void set_color(u8 color);
#endif