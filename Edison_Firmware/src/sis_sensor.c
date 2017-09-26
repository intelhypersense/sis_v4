// 2016-8-15

#include <stdio.h>
#include <strings.h>
#include <unistd.h>
#include <sys/types.h>
#include <sys/socket.h>
//#include <linux/in.h>
#include <stdlib.h>
#include <memory.h>
#include <arpa/inet.h>
#include <netinet/in.h>
#include <signal.h> //
#include "mraa.h"
#include <pthread.h>

#define CONNECT_TO_SERVER_DELAY 2
#define CONNECT_FAILED_MAX_COUNT 5
#define POWER_VALUE_MAX    110

#define RESEND_CMD              1

#define REC_HEX_DATA 1

#define USE_PRINTF 1

#define MIDI_RESEND 0

#define PORT   		 		     8887
#define UDP_ADV_PORT       8925
#define UDP_LISTEN_PORT    28925
#define UDP_LISTEN_BUFFER_LEN    1000

#define Buflen  10000//1024

#define USE_PLAYID 0

#define DELAY_MS 5

#define USE_SAME_FORMAT 1

#define PLAY_ID_8_12 1//0:  1-8,1:  1-12

#define MAX_PROCESS_NOTE_DATA 80//10*2*4       

#define PROCESS_MUTI_MIC_NOTE 1//

#define MIDI_HEAD_9X_8X 1//

const char note_data[]={
0,1,2,3,4,5,6,7,8,9,10,11,//1
12,13,14,15,16,17,18,19,20,21,22,23,//2
24,25,26,27,28,29,30,31,32,33,34,35,//3
36,37,38,39,40,41,42,43,44,45,46,47,//4
48,49,50,51,52,53,54,55,56,57,58,59,//5
60,61,62,63,64,65,66,67,68,69,70,71,//6
72,73,74,75,76,77,78,79,80,81,82,83,//7
84,85,86,87,88,89,90,91,92,93,94,95,//8
96,97,98,99,100,101,102,103,104,105,106,107,//9
108,109,110,111,112,113,114,115,116,117,118,119,//10
120,121,122,123,124,125,126,127};//11

#if PLAY_ID_8_12
	const char data_to_playid[]={
	1,2,3,4,5,6,7,8,9,10,11,12,//1
	1,2,3,4,5,6,7,8,9,10,11,12,//2
	1,2,3,4,5,6,7,8,9,10,11,12,//3
	1,2,3,4,5,6,7,8,9,10,11,12,//4
	1,2,3,4,5,6,7,8,9,10,11,12,//5
	1,2,3,4,5,6,7,8,9,10,11,12,//6
	1,2,3,4,5,6,7,8,9,10,11,12,//7
	1,2,3,4,5,6,7,8,9,10,11,12,//8
	1,2,3,4,5,6,7,8,9,10,11,12,//9
	1,2,3,4,5,6,7,8,9,10,11,12,//10
	1,2,3,4,5,6,7,8,9,10,11,12};//11
#else
	const char data_to_playid[]={
	1,1,2,2,3,4,4,5,5,6,6,7,//1
	1,1,2,2,3,4,4,5,5,6,6,7,//2
	1,1,2,2,3,4,4,5,5,6,6,7,//3
	1,1,2,2,3,4,4,5,5,6,6,7,//4
	1,1,2,2,3,4,4,5,5,6,6,7,//5
	1,1,2,2,3,4,4,5,5,6,6,7,//6
	1,1,2,2,3,4,4,5,5,6,6,7,//7
	1,1,2,2,3,4,4,5,5,6,6,7,//8
	1,1,2,2,3,4,4,5,5,6,6,7,//9
	1,1,2,2,3,4,4,5,5,6,6,7,//10
	1,1,2,2,3,4,4,5,5,6,6,7//11
	};
#endif

//char set_color[]="AT+LEDR";
//char set_color[]="AT+LEDG";
char set_color[]="AT+LEDB";
char black_color[]="AT+LEDD";

char server_closed=0;


#if USE_PLAYID
	char play_on_str[]="PLAYID=00";
#else
	#if USE_SAME_FORMAT
		char play_on_str[]="PON000";//
		char play_all_on_str1[] ="P+ON11P+ON12P+ON13P+ON14P+ON15P+ON16P+ON17P+ON18P+ON19P+ON1AP+ON1BP+ON1C";
		char play_all_off_str1[]="P+OF11P+OF12P+OF13P+OF14P+OF15P+OF16P+OF17P+OF18P+OF19P+OF1AP+OF1BP+OF1C";
		char play_all_on_str2[]= "P+ON21P+ON22P+ON23P+ON24P+ON25P+ON26P+ON27P+ON28P+ON29P+ON2AP+ON2BP+ON2C";
		char play_all_off_str2[]="P+OF21P+OF22P+OF23P+OF24P+OF25P+OF26P+OF27P+OF28P+OF29P+OF2AP+OF2BP+OF2C";
	#else
		char play_on_str[]="PLAYON=00";
	#endif
#endif

#if USE_SAME_FORMAT
	char play_off_str[]="POF000";
#else
	char play_off_str[]="PLAYOFF=00";
#endif

char motor_off_str[]="AT+OFF";

//uint8_t play_id;
//char play_on_off_flag;//1:on,0:off

mraa_uart_context uart;
int client_sockfd;  //
int udp_adv_sockfd;  //
int udp_listen_sockfd;  //

uint32_t HexStrToDec(uint8_t *str, int len);

#if PROCESS_MUTI_MIC_NOTE
	void midi_data_process(uint8_t *data,uint8_t data_count);
	void mic_data_process(uint8_t *data,uint8_t data_count);
#else
	void midi_data_process(uint8_t *data,uint8_t data_len);
	void mic_data_process(uint8_t *data);
#endif


void process_conn_client(int s);
void sig_pipe(int signo);    //
void set_test_color(uint8_t *data);
void test_all_device(uint8_t *data);

struct sockaddr_in udp_send_addr;
struct sockaddr_in udp_receive_addr;
struct sockaddr_in server_addr;
socklen_t addrlen;
int i,err;
//sighandler_t ret;
char server_ip[50] = "";
uint16_t tcp_server_port;
char udp_adv_packet[10]={0x55,0xBB,0x4E,0x4F,0x41,0x4C,0x41,0x42,0x53,0x00};
unsigned char udp_rx_packet[UDP_LISTEN_BUFFER_LEN];
int udp_rx_num;
char fBroadcast = 1;
char udp_step=1;
char udp_receive_time = 0;

int main(int argc,char *argv[])
{
	//
	mraa_gpio_context d_pin = NULL;
	d_pin = mraa_gpio_init_raw(165); //18-2
	mraa_gpio_dir(d_pin, MRAA_GPIO_OUT) ;
	mraa_gpio_write(d_pin, 1);
	printf( "1.8v out success!\n");

	//
	uart = mraa_uart_init(0);
	if (uart == NULL) {
		fprintf(stderr, "UART failed to setup\n");
		return EXIT_FAILURE;
	}
	mraa_uart_set_baudrate(uart, 115200);
	printf( "uart init success !\n");


	//int powerupsleep = 5; 	while(powerupsleep--) {sleep(1);} //test
	//while(1) {}

	usleep(10*1000);
	mraa_uart_write(uart, set_color, (sizeof(set_color)-1));
	usleep(100*1000);
	mraa_uart_write(uart, black_color, (sizeof(black_color)-1));
	printf( "set color success !\n");
	usleep(10*1000);
	mraa_uart_write(uart, motor_off_str, (sizeof(motor_off_str)-1));
	printf( "all motor off !\n");
	usleep(10*1000);

	addrlen=sizeof(struct sockaddr_in);

	/********************UDP ADV socket()*********************/
	udp_adv_sockfd= socket(AF_INET,SOCK_DGRAM,0);
	if(udp_adv_sockfd<0)
	{
		printf("client : create udp adv socket error\n");
		return -1;
	}
	else
	{
		printf("client : create udp adv socket success\n");
	}
	int optval = 1;//
	setsockopt ( udp_adv_sockfd,SOL_SOCKET,SO_BROADCAST | SO_REUSEADDR, &optval, sizeof (int) );

	memset(&udp_send_addr,0,sizeof(udp_send_addr));
	udp_send_addr.sin_family = AF_INET;
	udp_send_addr.sin_port = htons(UDP_ADV_PORT);//
#if 1
	udp_send_addr.sin_addr.s_addr = htonl(INADDR_BROADCAST); //255.255.255.255//set up router
#else
	udp_send_addr.sin_addr.s_addr = htonl(0xC0A82AFF); //192.168.42.255 //Edison as router
#endif


	/********************UDP LISTEN socket()*********************/
	udp_listen_sockfd= socket(AF_INET,SOCK_DGRAM,0);
	if(udp_listen_sockfd<0)
	{
		printf("client : create udp listen socket error\n");
		return -1;
	}
	else
	{
		printf("client : create udp listen socket success\n");
	}

	int set = 1;
	setsockopt(udp_listen_sockfd, SOL_SOCKET, SO_REUSEADDR, &set, sizeof(int));

	memset(&udp_receive_addr,0,sizeof(udp_receive_addr));
	udp_receive_addr.sin_family = AF_INET;
	udp_receive_addr.sin_port = htons(UDP_LISTEN_PORT);//
	udp_receive_addr.sin_addr.s_addr = htonl(INADDR_ANY);

	//
	if(bind( udp_listen_sockfd, (struct sockaddr *)&udp_receive_addr, sizeof(struct sockaddr))!=0)
	 {
	  printf("Can't bind socket to local port!\n");//
	  return -1;
	 }

  sleep(10); //wait 10sec wifi connected
  while(1)
  {
	switch(udp_step)
	{
		case 1:
			//usleep(1000*1000*20);
			/* */
			if(sendto(udp_adv_sockfd,udp_adv_packet,sizeof(udp_adv_packet),0,(struct sockaddr *)&udp_send_addr,addrlen)<0)
			{
				perror("Send ADV Failed:");
				//exit(1);
				//continue;
			}
			else
			{
				printf("client : send udp success\n");
				udp_step = 2; //go to receiver
				udp_receive_time = 0; //init
			}
			break;
		case 2:
			/* */
			printf("receiving UDP data \n");
			//udp_rx_num=recvfrom(udp_listen_sockfd,udp_rx_packet,UDP_LISTEN_BUFFER_LEN,0,(struct sockaddr *)&udp_receive_addr,&addrlen);//MSG_DONTWAIT
			udp_rx_num=recvfrom(udp_listen_sockfd,udp_rx_packet,UDP_LISTEN_BUFFER_LEN,MSG_DONTWAIT,(struct sockaddr *)&udp_receive_addr,&addrlen);//
			if(udp_rx_num==-1)
			{
				printf("receive data failed\n");
#if 0
				udp_step=1;
#else
				if(udp_receive_time<3)
				{
					sleep(1);
					udp_receive_time++; //receive 3 times
				}
				else
				{
					udp_step=1;
				}
#endif

			}
			//else//if((udp_rx_num=recvfrom(udp_listen_sockfd,udp_rx_packet,UDP_LISTEN_BUFFER_LEN,0,(struct sockaddr *)&udp_receive_addr,&addrlen))!=-1)
			else if(udp_rx_num>0)
			{
				printf("udp_rx_num:%d\n",udp_rx_num);
				printf("rec data:");
				for(i=0;i<udp_rx_num;i++)
				{
					printf("0x%x ",udp_rx_packet[i]);
				}
				printf("end\n");

				if((udp_rx_packet[0]==0x55)&&(udp_rx_packet[1]==0xbb)&&(udp_rx_packet[9]==0x00))
				{
					printf("check data ok\n");
					udp_step=3;
				}
				else
				{
					printf("check data failed,adv again\n");
					udp_step=1;
				}
			}
			break;
		case 3:
			tcp_server_port=(uint16_t)udp_rx_packet[7]<<8|(uint16_t)udp_rx_packet[6];
			sprintf(server_ip, "%d.%d.%d.%d", udp_rx_packet[5], udp_rx_packet[4],udp_rx_packet[3],udp_rx_packet[2]);
			//sprintf(server_ip, "%d.%d.%d.%d", udp_rx_packet[2], udp_rx_packet[3],udp_rx_packet[4],udp_rx_packet[5]);
			printf("tcp_port:%d\n",tcp_server_port);
			printf("tcp_ip:%s\n",server_ip);
			udp_step=4;
			break;
		case 4:
			/********************Creat TCP socket()*********************/
			client_sockfd= socket(AF_INET,SOCK_STREAM,0);
			if(client_sockfd<0)
			{
				printf("client : create socket error\n");
				//return 1;
			}
			else
			{
				udp_step=5;
				printf("client : create socket success\n");
			}
			/*******************connect server*********************/
			//
			memset(&server_addr,0,sizeof(server_addr));
			server_addr.sin_family = AF_INET;
			server_addr.sin_port = htons(tcp_server_port);
			server_addr.sin_addr.s_addr = htonl(INADDR_ANY);

			//server_addr.sin_addr.s_addr=inet_addr("192.168.1.253");//sever ip address
			server_addr.sin_addr.s_addr = inet_addr(server_ip);

			unsigned int conn2serv_delay=CONNECT_TO_SERVER_DELAY;
			unsigned char conn_count=0;

			printf("connecting server...\n");

			err = connect(client_sockfd,(struct sockaddr *)&server_addr,sizeof(struct sockaddr));
			if(err == 0)
			{
				printf("client : connect to server\n");
				udp_step=5;
			}
			else
			{
#if 0
				printf("client : connect error,reconnect,please wait...\n");
				while((err = connect(client_sockfd,(struct sockaddr *)&server_addr,sizeof(struct sockaddr)))!=0)
				{
					printf("client : connect error,reconnect after %d seconds\n",conn2serv_delay);
					sleep(CONNECT_TO_SERVER_DELAY);//
					conn_count++;
					if(conn_count>CONNECT_FAILED_MAX_COUNT)
					{
						break;
					}
				}

				if(conn_count>CONNECT_FAILED_MAX_COUNT)//
				{
					printf("client : connect to server failed,adv again\n");
					close(client_sockfd);
					printf("client : close tcp socket\n");
					udp_step=1;
				}
				else
				{
					udp_step=5;
					printf("client : connect to server\n");

				}
#else
				printf("client : connect to server failed,adv again\n");
				close(client_sockfd);
				printf("client : close tcp socket\n");
				udp_step=1;
#endif

			}
			break;
		case 5:
			process_conn_client(client_sockfd);
			udp_step=4;//
			#if 0//1
				mraa_uart_write(uart, black_color, (sizeof(black_color)-1));
				printf( "black color !\n");
				usleep(10*1000);
				mraa_uart_write(uart, motor_off_str, (sizeof(motor_off_str)-1));
				printf( "all motor off !\n");
				usleep(10*1000);
			#endif
			break;
		case 6:
			break;
		default:
			break;

	}
  }
	#if 0//1
		mraa_uart_write(uart, black_color, (sizeof(black_color)-1));
		printf( "black color !\n");
		usleep(10*1000);
		mraa_uart_write(uart, motor_off_str, (sizeof(motor_off_str)-1));
		printf( "all motor off !\n");
		usleep(10*1000);
	#endif
  printf("Close all sockets,return\n");
  close(udp_listen_sockfd);
  close(udp_adv_sockfd);
  close(client_sockfd);
  return 0;
}

void process_conn_client(int s)
{
    ssize_t size = 0;
    ssize_t tx_size = 0;
    ssize_t tx_count = 0;
    //char buffer[Buflen];

    uint8_t rec_data_pos=0;

    uint8_t buffer[Buflen];

    for(;;)
    {
        //memset(buffer,'\0',Buflen);
        /**/
        //size = read(0,buffer,Buflen);   //     

    	if(server_closed)//
    	{
    		server_closed=0;
    		break;
    	}

        size=recv(client_sockfd,buffer,Buflen,0);
        if(size >  0)
        {
			#if 0
            //
            write(s,buffer,strlen(buffer)+1);   //

            //
            for(size = 0 ; size == 0 ; size = read(s,buffer,Buflen) );

            write(1,buffer,strlen(buffer)+1);   //
			#else

			#if 0//
				#if REC_HEX_DATA
					for(tx_count=0;tx_count<size;tx_count++)
					{
						printf("%x ",buffer[tx_count]);
					}
					printf("\n");
				#else
					printf("sent:%s\n",buffer);
				#endif
			#endif

            if(size>MAX_PROCESS_NOTE_DATA)//
			{
            	size=MAX_PROCESS_NOTE_DATA;
			}

            //set_test_color((uint8_t *)buffer);
            //test_all_device((uint8_t *)buffer);

			#if PROCESS_MUTI_MIC_NOTE
            rec_data_pos=0;
            while(rec_data_pos<size)
            {
				midi_data_process((uint8_t *)buffer,rec_data_pos);
				mic_data_process((uint8_t *)buffer,rec_data_pos);
				rec_data_pos+=4;//
            }
			#else
            midi_data_process((uint8_t *)buffer,(uint8_t)size);
            mic_data_process((uint8_t *)buffer;
			#endif

			#if 0
            //if((tx_size=send(client_sockfd,buffer,strlen(buffer),0))>0)
            if((tx_size=send(client_sockfd,buffer,size,0))>0)
            {
				#if REC_HEX_DATA
            	for(tx_count=0;tx_count<tx_size;tx_count++)
            	{
            		printf("%x ",buffer[tx_count]);
            	}
            	printf("\n");
				#else
            	printf("sent:%s\n",buffer);
				#endif
            }
            else
            {
            	printf("send failed\n");
            }
			#endif
#endif
        }
        else
        {
        	perror("recv client data failed");
        	server_closed=1;//
        	close(client_sockfd);
        	printf("client_sockfd closed\n");
			//continue;
			//exit(EXIT_FAILURE);
        }
    }
}

void sig_pipe(int signo)    //
{
    printf("Catch a signal\n");
    if(signo == SIGTSTP)
    {

        printf("Receive SIGTSTP signal\n");
        int ret = close(client_sockfd);
        if(ret == 0)
            printf("success : socket closed\n");
        else if(ret ==-1 )
            printf("failed : socket unclosed\n");
        exit(1);
    }
}

uint32_t HexStrToDec(uint8_t *str, int len)
{
	uint32_t tmpResult = 0;
    //
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

void test_all_device(uint8_t *data)
{
	if(data[0]!=0xB0)
	{
		return;
	}
	switch(data[1])
	{
		case 0:
			mraa_uart_write(uart, play_all_off_str1, (sizeof(play_all_off_str1)-1));
			usleep(100*1000);
			mraa_uart_write(uart, play_all_off_str2, (sizeof(play_all_off_str2)-1));
			break;
		case 1:
			mraa_uart_write(uart, play_all_off_str2, (sizeof(play_all_off_str2)-1));
			usleep(100*1000);
			mraa_uart_write(uart, play_all_on_str1, (sizeof(play_all_on_str1)-1));
			break;
		case 2:
			mraa_uart_write(uart, play_all_off_str1, (sizeof(play_all_off_str1)-1));
			usleep(100*1000);
			mraa_uart_write(uart, play_all_on_str2, (sizeof(play_all_on_str2)-1));
			break;
		case 3:
			mraa_uart_write(uart, play_all_on_str1, (sizeof(play_all_on_str1)-1));
			usleep(100*1000);
			mraa_uart_write(uart, play_all_on_str2, (sizeof(play_all_on_str2)-1));
			break;
		default:
			break;
	}
}

void set_test_color(uint8_t *data)
{
	if(data[0]!=0xC0)
	{
		return;
	}
	switch(data[1])
	{
		case 0:
			set_color[6]='D';
			break;
		case 1:
			set_color[6]='R';
			break;
		case 2:
			set_color[6]='G';
			break;
		case 3:
			set_color[6]='B';
			break;
		case 4:
			set_color[6]='Y';
			break;
		case 5:
			set_color[6]='W';
			break;
		case 6:
			set_color[6]='P';
			break;
		default:
			set_color[6]='B';
			break;
	}
	#if USE_PRINTF
		printf("set color:%c\n",set_color[6]);
	#endif
	mraa_uart_write(uart, set_color, (sizeof(set_color)-1));

}

#if PROCESS_MUTI_MIC_NOTE
void midi_data_process(uint8_t *data,uint8_t data_count)
{
	uint8_t str_to_hex;
	uint8_t play_on_off_flag;
	uint8_t play_id;
	uint8_t play_id_H;
	uint8_t play_id_L;
	uint8_t group_id;
	uint8_t midi_play_id=0x88;
	float midi_power_value;//
	static uint8_t last_midi_play_id=0x88;
	static uint8_t last_play_state=2;
	static uint8_t last_play_id_L=0x88;

	#if MIDI_HEAD_9X_8X
	if(((*(data+data_count)&0xf0)!=0x80)&&((*(data+data_count)&0xf0)!=0x90))//A0 mic ·µ»Ø
	{
		return;
	}
	if((*(data+data_count)&0xf0)==0x90)
	{
		play_on_off_flag=1;
	}
	if((*(data+data_count)&0xf0)==0x80)
	{
		play_on_off_flag=0;
	}
	#else
	if((*(data+data_count)!=0x80)&&(*(data+data_count)!=0x90))
	{
		return;
	}
	if(*(data+data_count)==0x90)
	{
		play_on_off_flag=1;
	}
	if(*(data+data_count)==0x80)
	{
		play_on_off_flag=0;
	}
	#endif

	/*********************************/
	midi_power_value=*(data+data_count+2);//0-127
	if(midi_power_value>127)
		midi_power_value=127;

	else if(midi_power_value>POWER_VALUE_MAX)//
		midi_power_value=POWER_VALUE_MAX;

	midi_power_value=midi_power_value/127;
	midi_power_value=midi_power_value*99+1;//
	midi_power_value=(uint8_t)midi_power_value;//

	#if 0
	if(midi_power_value<30)
		midi_power_value=30;
	#endif

	group_id=*(data+data_count+3);

	str_to_hex=*(data+data_count+1);//

	play_id=data_to_playid[str_to_hex];
	if(last_play_id_L==0x88)//
		last_play_id_L=play_id;

	midi_play_id=(group_id<<4)+play_id;

	#if USE_PRINTF
	//printf("data,group,playid:%d,%d,%d\n",str_to_hex,group_id,play_id);
	#endif

	play_id_H=group_id+0x30;

	if(play_id>9)
	{
		play_id_L='A'+play_id%10;//
	}
	else
	{
		play_id_L=0x30+play_id%10;
	}

	if(play_on_off_flag)
	{
		#if USE_PRINTF
		printf("ID %d%x on!\n",group_id,play_id);
		#endif

		play_on_str[3]=play_id_H;
		play_on_str[4]=play_id_L;

		//
		if((group_id==3)||(group_id==4))
			play_on_str[4]='1';

		play_on_str[5]=midi_power_value;//

		mraa_uart_write(uart, play_on_str, (sizeof(play_on_str)-1));

		#if RESEND_CMD
		mraa_uart_write(uart, play_on_str, (sizeof(play_on_str)-1));
		#endif

		#if USE_PRINTF
			printf("play_on_str:%s\n",play_on_str);
			printf("midi_power_rec,midi_power_send:%d,%d\n",*(data+data_count+2),play_on_str[5]);
		#endif
	}
	else
	{
		#if USE_PRINTF
		printf("ID %d%x off!\n",group_id,play_id);
		#endif

		play_off_str[3]=play_id_H;
		play_off_str[4]=play_id_L;

		//
		if((group_id==3)||(group_id==4))
			play_off_str[4]='1';

		play_off_str[5]=midi_power_value;//

		mraa_uart_write(uart, play_off_str, (sizeof(play_off_str)-1));

		#if RESEND_CMD
		mraa_uart_write(uart, play_off_str, (sizeof(play_off_str)-1));
		#endif

		#if USE_PRINTF
			printf("play_off_str:%s\n",play_off_str);
			printf("midi_power_rec,midi_power_send:%d,%d\n",*(data+data_count+2),play_off_str[5]);
		#endif
	}

	last_midi_play_id=midi_play_id;
	last_play_state=play_on_off_flag;
	last_play_id_L=play_id;
	//printf("play_id_H,play_id_L:%c,%c\n",play_id_H,play_id_L);
}

#else
void midi_data_process(uint8_t *data,uint8_t data_len)
{
	uint8_t midi_data_count=0;
	uint8_t str_to_hex;
	uint8_t play_on_off_flag;
	uint8_t play_id;
	uint8_t play_id_H;
	uint8_t play_id_L;
	uint8_t group_id;
	uint8_t midi_play_id=0x88;
	static uint8_t last_midi_play_id=0x88;
	static uint8_t last_play_state=2;
	static uint8_t last_play_id_L=0x88;

	while(midi_data_count<data_len)
	{
		if((data[midi_data_count]!=0x80)&&(data[midi_data_count]!=0x90))//
		{
			midi_data_count=0;
			return;
		}
		else
		{
			if(data[midi_data_count]==0x90)
			{
				play_on_off_flag=1;
			}
			if(data[midi_data_count]==0x80)
			{
				play_on_off_flag=0;
			}
			group_id=data[midi_data_count+3];

			str_to_hex=data[midi_data_count+1];//

			play_id=data_to_playid[str_to_hex];
			if(last_play_id_L==0x88)//
				last_play_id_L=play_id;

			midi_play_id=(group_id<<4)+play_id;

			#if USE_PRINTF
			//printf("data,group,playid:%d,%d,%d\n",str_to_hex,group_id,play_id);
			#endif

			#if MIDI_RESEND //
			if((midi_play_id==last_midi_play_id)&&(play_on_off_flag==last_play_state))
			{
				last_midi_play_id=midi_play_id;
				last_play_state=play_on_off_flag;
				#if USE_PRINTF
					printf("same as last play\n");
				#endif
					return;
			}
			#endif

			#if 0
			//if((last_play_state!=0)&&(midi_play_id!=last_midi_play_id))//
			//if((midi_play_id!=last_midi_play_id))
			if((play_id!=last_play_id_L))//
			{
				//
				#if USE_PRINTF
				printf("new play,last ID %x off!\n",last_midi_play_id);
				#endif

				play_off_str[4]=(last_midi_play_id>>4)+0x30;//playidH
				play_off_str[5]=last_midi_play_id&0x0f;////playidL
				play_off_str[5]=(play_off_str[5]>9)?('A'+play_off_str[5]%10):(0x30+play_off_str[5]%10);

				#if USE_PRINTF
					printf("play_off_str:%s\n",play_off_str);
				#endif
				mraa_uart_write(uart, play_off_str, (sizeof(play_off_str)-1));

				#if RESEND_CMD
				mraa_uart_write(uart, play_off_str, (sizeof(play_off_str)-1));
				#endif

				#if !USE_SAME_FORMAT
				//usleep(10*1000);//
				usleep(DELAY_MS*1000);//
				#endif
			}
			#endif

			play_id_H=group_id+0x30;

			if(play_id>9)
			{
				play_id_L='A'+play_id%10;
			}
			else
			{
				play_id_L=0x30+play_id%10;
			}

			if(play_on_off_flag)
			{
				#if USE_PRINTF
				printf("ID %d%x on!\n",group_id,play_id);
				#endif

				play_on_str[4]=play_id_H;
				play_on_str[5]=play_id_L;

				mraa_uart_write(uart, play_on_str, (sizeof(play_on_str)-1));

				#if RESEND_CMD
				mraa_uart_write(uart, play_on_str, (sizeof(play_on_str)-1));
				#endif

				#if USE_PRINTF
					printf("play_on_str:%s\n",play_on_str);
				#endif
			}
			else
			{
				#if USE_PRINTF
				printf("ID %d%x off!\n",group_id,play_id);
				#endif

				play_off_str[4]=play_id_H;
				play_off_str[5]=play_id_L;

				mraa_uart_write(uart, play_off_str, (sizeof(play_off_str)-1));

				#if RESEND_CMD
				mraa_uart_write(uart, play_off_str, (sizeof(play_off_str)-1));
				#endif

				#if USE_PRINTF
					printf("play_off_str:%s\n",play_off_str);
				#endif
			}

			last_midi_play_id=midi_play_id;
			last_play_state=play_on_off_flag;
			last_play_id_L=play_id;
			//printf("play_id_H,play_id_L:%c,%c\n",play_id_H,play_id_L);


			midi_data_count+=4;

		}
	}
}
#endif

#if PROCESS_MUTI_MIC_NOTE
void mic_data_process(uint8_t *data,uint8_t data_count)
{
	static uint8_t str_to_hex12[3];
	uint8_t str_to_hex34;
	float mic_power_value;
	static uint8_t play_id[3];
	uint8_t mic_group_id;
	static uint8_t play_id_L[3];
	static uint8_t play_id_H[5];
	static uint8_t last_play_id_L[3];
	static uint8_t last_play_id_H[3];
	static uint8_t last_play_id[3]={0x88,0x88,0x88};
	//static uint8_t last_play_id34=0x88;
	if(*(data+data_count)!=0xA0)// 	
    {
		return;
	}
	mic_group_id=*(data+data_count+3);
	play_id_H[mic_group_id]=*(data+data_count+3)+0x30;

	/*********************************/
	mic_power_value=*(data+data_count+2);//  0-127
	if(mic_power_value>127)
		mic_power_value=127;

	else if(mic_power_value>POWER_VALUE_MAX)//
		mic_power_value=POWER_VALUE_MAX;

	mic_power_value=mic_power_value/127;
	mic_power_value=mic_power_value*99+1;//-100
	mic_power_value=(uint8_t)mic_power_value;//

	#if 0
	if(mic_power_value<30)
		mic_power_value=30;
	#endif

	if(mic_group_id<=2)//1,2
	{
		//str_to_hex12=HexStrToDec(&(data[3]),2);// 
		str_to_hex12[mic_group_id]=*(data+data_count+1);
		if(str_to_hex12[mic_group_id]==0)
		{
			//
#if USE_PRINTF
			printf("Group %d all off!\n",mic_group_id);
#endif
			play_off_str[3]=last_play_id_H[mic_group_id];
			play_off_str[4]=last_play_id_L[mic_group_id];

			play_off_str[5]=mic_power_value;

			mraa_uart_write(uart, play_off_str, (sizeof(play_off_str)-1));

			#if RESEND_CMD
			//mraa_uart_write(uart, play_off_str, (sizeof(play_off_str)-1));
			#endif

			return;
		}

		play_id[mic_group_id]=(mic_group_id<<4)+str_to_hex12[mic_group_id];
#if USE_PRINTF
		//printf("group,note,play_id:%d,%d,%x\n",mic_group_id,str_to_hex12[mic_group_id],play_id);
#endif

		if(last_play_id[mic_group_id]!=play_id[mic_group_id])//
		{
			//
#if USE_PRINTF
			printf("ID %x off:%d!\n",last_play_id[mic_group_id],(uint8_t)mic_power_value);
#endif

			play_off_str[3]=last_play_id_H[mic_group_id];
			play_off_str[4]=last_play_id_L[mic_group_id];

			play_off_str[5]=mic_power_value;

			mraa_uart_write(uart, play_off_str, (sizeof(play_off_str)-1));

			#if RESEND_CMD
			mraa_uart_write(uart, play_off_str, (sizeof(play_off_str)-1));
			#endif

			#if !USE_SAME_FORMAT
			//usleep(10*1000);//
			usleep(DELAY_MS*1000);//
			#endif

			//
			if(str_to_hex12[mic_group_id]>9)
			{
				play_id_L[mic_group_id]='A'+str_to_hex12[mic_group_id]%10;
			}
			else
			{
				play_id_L[mic_group_id]=0x30+str_to_hex12[mic_group_id]%10;
			}
#if USE_PRINTF
			printf("ID %x on:%d!\n",play_id[mic_group_id],(uint8_t)mic_power_value);
#endif

			play_on_str[3]=play_id_H[mic_group_id];
			play_on_str[4]=play_id_L[mic_group_id];

			play_on_str[5]=mic_power_value;

			mraa_uart_write(uart, play_on_str, (sizeof(play_on_str)-1));

			#if RESEND_CMD
			mraa_uart_write(uart, play_on_str, (sizeof(play_on_str)-1));
			#endif

			last_play_id_H[mic_group_id]=play_id_H[mic_group_id];
			last_play_id_L[mic_group_id]=play_id_L[mic_group_id];
			last_play_id[mic_group_id]=play_id[mic_group_id];
		}

		#if USE_PLAYID
		else
		{
#if USE_PRINTF
			printf("ID %x on!\n",last_play_id);
#endif
			play_on_str[7]=last_play_id_H;
			play_on_str[8]=last_play_id_L;
			mraa_uart_write(uart, play_on_str, (sizeof(play_on_str)-1));
		}
		#endif

	}
	else if(mic_group_id<=4)//3,4
	{
		str_to_hex34=*(data+data_count+1);
		if(str_to_hex34)
		{
#if USE_PRINTF
			printf("Energy on: %d!\n",mic_group_id);
#endif

			play_on_str[3]=play_id_H[mic_group_id];
			play_on_str[4]='1';

			play_on_str[5]=mic_power_value;

			mraa_uart_write(uart, play_on_str, (sizeof(play_on_str)-1));

			#if RESEND_CMD
			mraa_uart_write(uart, play_on_str, (sizeof(play_on_str)-1));
			#endif
		}
		else
		{
#if USE_PRINTF
			printf("Energy off: %d!\n",mic_group_id);
#endif
			play_off_str[3]=play_id_H[mic_group_id];
			play_off_str[4]='1';

			play_off_str[5]=mic_power_value;

			mraa_uart_write(uart, play_off_str, (sizeof(play_off_str)-1));

			#if RESEND_CMD
			mraa_uart_write(uart, play_off_str, (sizeof(play_off_str)-1));
			#endif
		}
	}
	else
	{
		printf("group id error\n");
	}
}
#else
void mic_data_process(uint8_t *data)
{
	static uint8_t str_to_hex12[3];
	uint8_t str_to_hex34;
	static uint8_t play_id[3];
	uint8_t mic_group_id;
	static uint8_t play_id_L[3];
	static uint8_t play_id_H[5];
	static uint8_t last_play_id_L[3];
	static uint8_t last_play_id_H[3];
	static uint8_t last_play_id[3]={0x88,0x88,0x88};
	//static uint8_t last_play_id34=0x88;
	if(data[0]!=0xA0)//
	{
		return;
	}
	mic_group_id=data[3];
	play_id_H[mic_group_id]=data[3]+0x30;

	if(mic_group_id<=2)//1,2
	{
		//str_to_hex12=HexStrToDec(&(data[3]),2);// 
		str_to_hex12[mic_group_id]=data[1];
		if(str_to_hex12[mic_group_id]==0)
		{
			//,
#if USE_PRINTF
			printf("Group %d all off!\n",mic_group_id);
#endif
			play_off_str[4]=last_play_id_H[mic_group_id];
			play_off_str[5]=last_play_id_L[mic_group_id];

			mraa_uart_write(uart, play_off_str, (sizeof(play_off_str)-1));

			#if RESEND_CMD
			//mraa_uart_write(uart, play_off_str, (sizeof(play_off_str)-1));
			#endif

			return;
		}

		play_id[mic_group_id]=(mic_group_id<<4)+str_to_hex12[mic_group_id];
#if USE_PRINTF
		//printf("group,note,play_id:%d,%d,%x\n",mic_group_id,str_to_hex12[mic_group_id],play_id);
#endif

		if(last_play_id[mic_group_id]!=play_id[mic_group_id])//
		{
			//
#if USE_PRINTF
			printf("ID %x off!\n",last_play_id[mic_group_id]);
#endif

			play_off_str[4]=last_play_id_H[mic_group_id];
			play_off_str[5]=last_play_id_L[mic_group_id];

			mraa_uart_write(uart, play_off_str, (sizeof(play_off_str)-1));

			#if RESEND_CMD
			mraa_uart_write(uart, play_off_str, (sizeof(play_off_str)-1));
			#endif

			#if !USE_SAME_FORMAT
			//usleep(10*1000);//
			usleep(DELAY_MS*1000);//
			#endif

			//
			if(str_to_hex12[mic_group_id]>9)
			{
				play_id_L[mic_group_id]='A'+str_to_hex12[mic_group_id]%10;
			}
			else
			{
				play_id_L[mic_group_id]=0x30+str_to_hex12[mic_group_id]%10;
			}
#if USE_PRINTF
			printf("ID %x on!\n",play_id[mic_group_id]);
#endif

			play_on_str[4]=play_id_H[mic_group_id];
			play_on_str[5]=play_id_L[mic_group_id];

			mraa_uart_write(uart, play_on_str, (sizeof(play_on_str)-1));

			#if RESEND_CMD
			mraa_uart_write(uart, play_on_str, (sizeof(play_on_str)-1));
			#endif

			last_play_id_H[mic_group_id]=play_id_H[mic_group_id];
			last_play_id_L[mic_group_id]=play_id_L[mic_group_id];
			last_play_id[mic_group_id]=play_id[mic_group_id];
		}

		#if USE_PLAYID
		else
		{
#if USE_PRINTF
			printf("ID %x on!\n",last_play_id);
#endif
			play_on_str[7]=last_play_id_H;
			play_on_str[8]=last_play_id_L;
			mraa_uart_write(uart, play_on_str, (sizeof(play_on_str)-1));
		}
		#endif

	}
	else if(mic_group_id<=4)//3,4
	{
		str_to_hex34=data[1];
		if(str_to_hex34)
		{
#if USE_PRINTF
			printf("Energy on: %d!\n",mic_group_id);
#endif

			play_on_str[4]=play_id_H[mic_group_id];
			play_on_str[5]='1';

			mraa_uart_write(uart, play_on_str, (sizeof(play_on_str)-1));

			#if RESEND_CMD
			mraa_uart_write(uart, play_on_str, (sizeof(play_on_str)-1));
			#endif
		}
		else
		{
#if USE_PRINTF
			printf("Energy off: %d!\n",mic_group_id);
#endif
			play_off_str[4]=play_id_H[mic_group_id];
			play_off_str[5]='1';

			mraa_uart_write(uart, play_off_str, (sizeof(play_off_str)-1));

			#if RESEND_CMD
			mraa_uart_write(uart, play_off_str, (sizeof(play_off_str)-1));
			#endif
		}
	}
	else
	{
		printf("group id error\n");
	}
}
#endif









