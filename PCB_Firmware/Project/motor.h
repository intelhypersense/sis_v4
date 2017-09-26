#ifndef __MOTOR_H
#define __MOTOR_H

#include "stm8s.h"

//#define MOTOR_PWM_PERIOD 2000//2MS 500HZ

//#define MOTOR_PWM_PERIOD 4000//4MS 250HZ

#define MOTOR_PWM_PERIOD 10000//10MS 100HZ

//#define MOTOR_PWM_PERIOD 20000//20MS 50HZ

//#define MOTOR_PWM_PERIOD 40000//40MS 25HZ

//#define MOTOR_PWM_PERIOD 50000//50MS 20HZ

//#define MOTOR_PWM_PERIOD 65000//65MS 15.38HZ

#define MOTOR_PORT GPIOA	
#define MOTOR_PIN  GPIO_PIN_3

/*
#define LED_PORT_SEL GPIOC	
#define LED_PIN_SEL  GPIO_PIN_7
*/
#define LED_PORT_SEL GPIOD	
#define LED_PIN_SEL  GPIO_PIN_3

#define MOTOR_ON "AT+ON"
#define MOTOR_OFF "AT+OFF"

void TIM2_Config(u16 CCR3_Val);
void Motor_Off(void);
//void Motor_On(void);
void Motor_On(uint8_t duty_cycle);//duty_cycle：占空比数值
void Motor_Init(void);

void Led_Init(void);

#endif