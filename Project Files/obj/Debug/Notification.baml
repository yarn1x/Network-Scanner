   M S B A M L   `   `   `  ����P  LАнализ Сети, Version=0.0.1.0, Culture=neutral, PublicKeyToken=null'clr-namespace:NetScanner
NetScanner  5   	       NetScanner.Notification   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   *localclr-namespace:NetScanner     5   	   $	��300��5   	   $	��400��5	   	   $	��400��5
   	   $	��300��6   $	��400��5   	   $	��300��6     � WindowStyle$
  None=�5   	    � AllowsTransparency . 
������  � WindowStartupLocation$ CenterScreen=�5   	    � 	Resources 5         FocusVisual �� Property �� Value �� StrokeDashArray J� SnapsToDevicePixels  Static.Foreground �� Color  Static.Background  Static.Border  MouseOver.Background  Pressed.Background  ButtonStyle_smooth	 �� 
TargetType
 �� 
TargetType ��Name �� CornerRadius ��Name �� Triggers P� Property P� Value �� 
TargetName��%e  &	        &	 �     &	 �     &	 �     &	     &	 8    &	 [    7   7  7  7  7  7  �� 5   
   .�5      ��  � �� 5      �� 5      .R�5      �� $��2q�6$   $	 1 2R�6/   #��� ��6E    . $��1��6�   5      5      5      �� 5   
   $ 	#FFD8D8D8��64   �� 5   
   $ 	#FF3E3E3E��64   �� 5   
   $ Transparent��60   �� 5   
   $ 	#FF505050��67   �� 5    
   $ 	#FF616161��65   �� 5#   
   "	 ��6+   .�5$      ��  �@�FocusVisualStyle8   61   �� 5%       � ��8  6+   �� 5&       � ��8  6,   �� 5'       � ��8  6+   �� 5(       � ��$ 1q�60   �� 5)       � ��$ Center=�6;   �� 5*       � ��$ Center=�69   �� 5+       � ��$ 1q�6(   �� 5,       � �� 5-      �� 5.      "
 ��6&   .R�5/      �� $ border��50      $ 10��5/   !   #��z��50      #��z��6A   #��z��6m    . ._�51      �� $ contentPresenter��6/   |�.  #��z��6[   #��z��6�   ��. # z 6�   #��z��6  52       53      P� 54       �@��IsDefaulted . . �55   "   �� $ border��6@   # � ��6T    � ��56      P� 57       �@J�IsMouseOver . . �58   "   �� $ border��6?   8   � ��6S   59      P� 5:       � �� . . �5;   "   �� $ border��6?   8   � ��6S   5<      5>      5@      5A      5D      .��5F      � -   5         �Name$ Grid_window��5F      .+�5H   
   �� ���	#FF202023$ 30��5I      5J      �� 5L   
   ���	#FFD8D8D8$ 30��5M      $��5q�5N      ._�5P      �� -   5        $ Border_dragArea��5P      $��	0,0,0,251q�5Q      ���	#FFD8D8D8$ 20, 20, 0, 0��5S      5U      5W      �� 5Y   
   -   5         ��Name$ 	tb_header��5Y      $��Header��6'   
���   �$	��Top=�5[      $
��Left=�5\      $��24(�5]      $��Bahnschrift Light)�5^      $��	23,10,0,0q�5_      6'   �� 5b   
   -   5        $ tb_text��5b      $��Notification��6%   $
��Wrap=�5c      ���	#FFD8D8D8$	��Top=�5e      $��Center=�5f      $��24(�5g      $��Bahnschrift Light)�5h      $��0,69,0,0q�63   $	��358��6E   $	��169��6T   �� 5j   
   -   5         ��Name$ Btn_ok��5j      ���	#FFD8D8D8$
��ОК��5l      $��Bahnschrift Light)�5m      $	��100��6=   $��50��6I   $��Bottom=�6U   $��Right=�6p   $��	0,0,20,20q�6�    � Cursor$
 Hand��6�    �� 	IsDefault . #���  6�   5n      5p      5r      