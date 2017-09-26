#include "motor.h"


void TIM2_Config(u16 CCR3_Val)
{
  //CLK_PeripheralClockConfig(CLK_PERIPHERAL_TIMER2,ENABLE);
  CLK->PCKENR1 |= 0x20;
  /* Time base configuration */
  //TIM2_TimeBaseInit(TIM2_PRESCALER_1, 999);
  //TIM2_TimeBaseInit(TIM2_PRESCALER_16, 999);//1MS  1KHZ
  //TIM2_TimeBaseInit(TIM2_PRESCALER_16, 1999);//2MS  500HZ
  
#if 0
  TIM2_TimeBaseInit(TIM2_PRESCALER_16, (MOTOR_PWM_PERIOD-1));//2MS  500HZ

  /* PWM1 Mode configuration: Channel2 */ 
  TIM2_OC2Init(TIM2_OCMODE_PWM1, TIM2_OUTPUTSTATE_ENABLE,CCR3_Val, TIM2_OCPOLARITY_HIGH);
  TIM2_OC2PreloadConfig(ENABLE);
  
  /* PWM1 Mode configuration: Channel3 */         
  TIM2_OC3Init(TIM2_OCMODE_PWM1, TIM2_OUTPUTSTATE_ENABLE,CCR3_Val, TIM2_OCPOLARITY_HIGH);
  TIM2_OC3PreloadConfig(ENABLE);

  TIM2_ARRPreloadConfig(ENABLE);

  /* TIM2 enable counter */
  TIM2_Cmd(ENABLE);
#else
  
  /* Set the Prescaler value */
  TIM2->PSCR = (uint8_t)(TIM2_PRESCALER_16);
  /* Set the Autoreload value */
  TIM2->ARRH = (uint8_t)((MOTOR_PWM_PERIOD-1) >> 8);
  TIM2->ARRL = (uint8_t)(MOTOR_PWM_PERIOD-1);
  
  /************************PWM1 Mode configuration: Channel2 *****************************/ 
  
  /* Disable the Channel 1: Reset the CCE Bit, Set the Output State, the Output Polarity */
  TIM2->CCER1 &= (uint8_t)(~( TIM2_CCER1_CC2E |  TIM2_CCER1_CC2P ));
  /* Set the Output State & Set the Output Polarity */
  TIM2->CCER1 |= (uint8_t)((uint8_t)(TIM2_OUTPUTSTATE_ENABLE  & TIM2_CCER1_CC2E ) |
                           (uint8_t)(TIM2_OCPOLARITY_HIGH & TIM2_CCER1_CC2P));
  
  /* Reset the Output Compare Bits & Set the Output Compare Mode */
  TIM2->CCMR2 = (uint8_t)((uint8_t)(TIM2->CCMR2 & (uint8_t)(~TIM2_CCMR_OCM)) | 
                          (uint8_t)TIM2_OCMODE_PWM1);
  
  /* Set the Pulse value */
  TIM2->CCR2H = (uint8_t)(CCR3_Val >> 8);
  TIM2->CCR2L = (uint8_t)(CCR3_Val);
  
  /* Enables or disables the TIM2 peripheral Preload Register on CCR2. */
  TIM2->CCMR2 |= (uint8_t)TIM2_CCMR_OCxPE;
  
  /************************PWM1 Mode configuration: Channel3 *****************************/ 
  
  TIM2->CCER2 &= (uint8_t)(~( TIM2_CCER2_CC3E  | TIM2_CCER2_CC3P));
  /* Set the Output State & Set the Output Polarity */
  TIM2->CCER2 |= (uint8_t)((uint8_t)(TIM2_OUTPUTSTATE_ENABLE & TIM2_CCER2_CC3E) |  
                           (uint8_t)(TIM2_OCPOLARITY_HIGH & TIM2_CCER2_CC3P));
  
  /* Reset the Output Compare Bits & Set the Output Compare Mode */
  TIM2->CCMR3 = (uint8_t)((uint8_t)(TIM2->CCMR3 & (uint8_t)(~TIM2_CCMR_OCM)) |
                          (uint8_t)TIM2_OCMODE_PWM1);
  
//震动强度最低设置为20，太小了感觉不到
#if 0
  if((CCR3_Val<(MOTOR_PWM_PERIOD/100)*99-1)&&(CCR3_Val>(MOTOR_PWM_PERIOD/100)*80))//保证最小的震动强度为20（为了与灯的亮度匹配），且为0不震动
  {
    CCR3_Val=(MOTOR_PWM_PERIOD/100)*80;
  }
#else
  if((CCR3_Val<9899)&&(CCR3_Val>8000))//保证最小的震动强度为20（为了与灯的亮度匹配），且为0不震动
  {
    CCR3_Val=8000;
  }
#endif
  
  /* Set the Pulse value */
  TIM2->CCR3H = (uint8_t)(CCR3_Val >> 8);
  TIM2->CCR3L = (uint8_t)(CCR3_Val);
  
  /* Enables or disables the TIM2 peripheral Preload Register on CCR3. */
  TIM2->CCMR3 |= (uint8_t)TIM2_CCMR_OCxPE;
  
  /* Enables or disables TIM2 peripheral Preload register on ARR. */
  TIM2->CR1 |= (uint8_t)TIM2_CR1_ARPE;
  
  
  /* TIM2 enable counter */
  TIM2->CR1 |= (uint8_t)TIM2_CR1_CEN;
#endif
}

void Led_Init(void)
{
  GPIO_Init(LED_PORT_SEL, LED_PIN_SEL, GPIO_MODE_OUT_PP_HIGH_FAST);	
}

void Led_On(uint8_t led_pwm_duty_cycle)
{
  uint16_t CCR2_val;
  CCR2_val=(MOTOR_PWM_PERIOD/100)*led_pwm_duty_cycle-1;
  
  /* PWM1 Mode configuration: Channel2 */ 
  TIM2_OC2Init(TIM2_OCMODE_PWM1, TIM2_OUTPUTSTATE_ENABLE,CCR2_val, TIM2_OCPOLARITY_HIGH);
  TIM2_OC2PreloadConfig(ENABLE);
}

void Led_Off(void)
{
  //TIM2_ForcedOC2Config(TIM2_FORCEDACTION_ACTIVE);//To ensure the motor is off,this is a must
  TIM2->CCMR2 = (uint8_t)((uint8_t)(TIM2->CCMR2 & (uint8_t)(~TIM2_CCMR_OCM))  
                          | (uint8_t)TIM2_FORCEDACTION_ACTIVE);
  
  //GPIO_Init(LED_PORT_SEL, LED_PIN_SEL, GPIO_MODE_OUT_PP_HIGH_FAST);
  /* Reset corresponding bit to GPIO_Pin in CR2 register */
  LED_PORT_SEL->CR2 &= (uint8_t)(~(LED_PIN_SEL));
  /* Output high */
  LED_PORT_SEL->ODR |= (uint8_t)LED_PIN_SEL;
  /* Set Output mode */
  LED_PORT_SEL->DDR |= (uint8_t)LED_PIN_SEL;
  /* Pull-Up or Push-Pull */
  LED_PORT_SEL->CR1 |= (uint8_t)LED_PIN_SEL;
  /* 10M */
  LED_PORT_SEL->CR2 |= (uint8_t)LED_PIN_SEL;
}

void Motor_Init(void)
{
  GPIO_Init(MOTOR_PORT, MOTOR_PIN, GPIO_MODE_OUT_PP_HIGH_FAST);	
}

void Motor_Off(void)
{
#if 0
  TIM2_Cmd(DISABLE);
  TIM2_ForcedOC3Config(TIM2_FORCEDACTION_ACTIVE);//To ensure the motor is off,this is a must
  GPIO_Init(MOTOR_PORT, MOTOR_PIN, GPIO_MODE_OUT_PP_HIGH_FAST);
  //GPIO_WriteHigh(MOTOR_PORT, MOTOR_PIN);
#else
  TIM2->CR1 &= (uint8_t)(~TIM2_CR1_CEN);
  TIM2->CCMR3  =  (uint8_t)((uint8_t)(TIM2->CCMR3 & (uint8_t)(~TIM2_CCMR_OCM))
                            | (uint8_t)TIM2_FORCEDACTION_ACTIVE);
  
  
  /* Reset corresponding bit to GPIO_Pin in CR2 register */
  MOTOR_PORT->CR2 &= (uint8_t)(~(MOTOR_PIN));
  /* Output high */
  MOTOR_PORT->ODR |= (uint8_t)MOTOR_PIN;
  /* Set Output mode */
  MOTOR_PORT->DDR |= (uint8_t)MOTOR_PIN;
  /* Pull-Up or Push-Pull */
  MOTOR_PORT->CR1 |= (uint8_t)MOTOR_PIN;
  /* 10M */
  MOTOR_PORT->CR2 |= (uint8_t)MOTOR_PIN;
  
  //TIM2_ForcedOC2Config(TIM2_FORCEDACTION_ACTIVE);//To ensure the motor is off,this is a must
  TIM2->CCMR2 = (uint8_t)((uint8_t)(TIM2->CCMR2 & (uint8_t)(~TIM2_CCMR_OCM))  
                          | (uint8_t)TIM2_FORCEDACTION_ACTIVE);
  
  //GPIO_Init(LED_PORT_SEL, LED_PIN_SEL, GPIO_MODE_OUT_PP_HIGH_FAST);
  /* Reset corresponding bit to GPIO_Pin in CR2 register */
  LED_PORT_SEL->CR2 &= (uint8_t)(~(LED_PIN_SEL));
  /* Output high */
  LED_PORT_SEL->ODR |= (uint8_t)LED_PIN_SEL;
  /* Set Output mode */
  LED_PORT_SEL->DDR |= (uint8_t)LED_PIN_SEL;
  /* Pull-Up or Push-Pull */
  LED_PORT_SEL->CR1 |= (uint8_t)LED_PIN_SEL;
  /* 10M */
  LED_PORT_SEL->CR2 |= (uint8_t)LED_PIN_SEL;
  
#endif
}

//void Motor_On(void)
void Motor_On(uint8_t duty_cycle)//duty_cycle：占空比数值
{
  uint16_t pulse_high_time;
  //pulse_high_time=(MOTOR_PWM_PERIOD/100)*duty_cycle-1;
  pulse_high_time=100*duty_cycle-1;
  TIM2_Config(pulse_high_time);
  //GPIO_WriteLow(MOTOR_PORT, MOTOR_PIN);
  //TIM2_Config(1000);
  //GPIO_Init(MOTOR_PORT, MOTOR_PIN, GPIO_MODE_OUT_PP_LOW_FAST);	
}