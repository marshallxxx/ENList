# ENList
Simple UserControl in Silverlight which help provide pull to refresh action.

####How to use it

1 Where you want a PulToRefreshListBox import in your .xaml EN.ListUpdater namespace
```
<phone:PhoneApplicationPage
    x:Class="YourClass"
    ...
    xmlns:enlist="clr-namespace:EN.ListUpdater"
    ...
```

2 Than create PulToRefreshListBox user control in .xaml
```
<!--ContentPanel - place additional content here-->
  <enlist:PulToRefreshListBox x:Name="ENList" Grid.Row="1">
            
            <!-- Inser here your custom listbox -->
            <enlist:PulToRefreshListBox.ListBoxComponent>
                <ListBox x:Name="ListBox" />
        </enlist:PulToRefreshListBox.ListBoxComponent>
       
        <enlist:PulToRefreshListBox.HeaderComponnet>
                
            <!-- Inser here your custom header view -->
            <StackPanel>
                <TextBlock x:Name="In " Text="Pull to refresh" TextAlignment="Center" />
            </StackPanel>
                
        </enlist:PulToRefreshListBox.HeaderComponnet>
  </enlist:PulToRefreshListBox>
  ```

  3 Set ItemsSource for ListBox
```
ENList.ListBoxComponent.ItemsSource = new string[] { "Hello", "Bonjour", "Salut", "Ciao", "Holla" };

//Subscribe to PullToRefreshListBox events
ENList.OnHeaderFullHeight += ENList_OnHeaderFullHeight;
ENList.OnHeaderHidden += ENList_OnHeaderHidden;
ENList.OnActivatedDrop += ENList_OnActivatedDrop;
```

###Events 
####OnHeaderFullHeight
Is triggered when HeaderView is fully visible

####OnHeaderHidden
Is triggered when HeaderView was fully visible and than it becomes not fully visible

####OnActivatedDrop
Is triggered when HeaderView was fully visible and drag gesture was released

####Also can customize
