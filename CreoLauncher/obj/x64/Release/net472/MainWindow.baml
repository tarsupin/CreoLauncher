   M S B A M L   `   `   `  ���� G  CCreoLauncher, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:CreoLauncherCreoLauncher      CreoLauncher.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     ,localclr-namespace:CreoLauncher     -     � 
ResizeMode$  CanMinimize=� � WindowStartupLocation$ CenterScreen=� � Title$ Creo Launcher��$	��600��$
��1080��.��� ����  �� EndPoint$ 0.5,1&� �� 
StartPoint$ 0.5,0&�.���  � Color$ 	#FF8DC8F1�� � Offset$ 0Q�� $ 	#FFE1F7FF��$ 1Q�� $ 	#FF95C6EC��$ 0.502Q�.+��� -    ��Name$ 
TitleLabel��$��
Creo Nexus��$
��Left=�$��	10,10,0,0q�$	��Top=�$��41��$	��155��$��22(�$
��Bold"��� -   $ VersionLabel��$��Version 0.0.0��$
��Left=�$��
150,21,0,0q�$	��Top=�$��30��$	��132���� -   $ StatusLabel��$��Updating Game��$y�Visible=�$��Center=�$��0, 0, 0, 10q�$��38��$��Center=�$	��280��$	��400��$��38��$��Bottom=�$��24(�$
��Bold"����	#66FFEB23$��	10,2,10,0q�(  "System.Windows.Controls.WebBrowser  $��Center=�$��10,51,10,60q�$	��Top=�   Source$( "https://creo.nexus.games/news.html?��� -   	 ��Name$	 ButtonWebsite��$��Launch Website��$
��Left=�$��	10,0,0,10q�$��Bottom=�$��38��$	��242�����	#FF415ACB
�������$
��Bold"�
�������$��0,0,0,0q�$��2��
 � Cursor$

 Hand�� � 	Resources  �� 
TargetType �� Property �� Value��%9   '��       �� " ��.���  �@��CornerRadius$ 5������ " ��.���  � �� ��  �� 
TargetType" ��.R���  ��Name$ Border��#��z��._��� $��Center=�$��Center=� �� Triggers P�  P� Property �@J�IsMouseOver P� Value$
 True��. ���  �� 
TargetName$ Border�� � �� �	#FF4C87C1�� -   $	 
ButtonPlay��$��	Play Game��$��Right=�$��	0,0,10,10q�$��Bottom=�$��38��$	��242�����	#FF08761C
�������$
��Bold"�
�������$��0,0,0,0q�$��1�� J� OpacityMask
 �   �$

 Hand�� ��%9   '��       �� " ��.���  �@��CornerRadius$ 5������ " ��.���  � �� �� " ��.R��� $ Border��#��z��._��� $��Center=�$��Center=� P�  �@J�IsMouseOver$
 True��. ��� $ Border�� � �� �	#FF2D8F59