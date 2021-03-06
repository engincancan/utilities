Function global:Out-ConsoleList {

    param($LastWord='',$ReturnWord,[switch]$forceList)
    if (-not $ReturnWord ) {$ReturnWord = $LastWord}
    $Content = @($input)
     
    # Not Show List just forward input in case less then minimum options

    if (($Content.Length -lt $MinimumListItems) -and (-not $ForceList)){Return $Content}

# Load helper functions
 
Function new-Box (
  [Drawing.Size]$size,
  $ForegroundColor = $Host.UI.RawUI.ForegroundColor,
  $BackgroundColor = $Host.UI.RawUI.BackgroundColor
) {

  $BoxChars = @{
    'HorizontalDouble' = ([char]9552).ToString()
    'VerticalDouble' = ([char]9553).ToString()
    'TopLeftDouble' = ([char]9556).ToString()
    'TopRightDouble' = ([char]9559).ToString()
    'BottomLeftDouble' = ([char]9562).ToString()
    'BottomRightDouble' = ([char]9565).ToString()
    'HorizontalDoubleSingleDown' = ([char]9572).ToString()
    'Horizontal' = ([char]9472).ToString()
    'Vertical' = ([char]9474).ToString()
    'TopLeft' = ([char]9484).ToString()
    'TopRight' = ([char]9488).ToString()
    'BottomLeft' = ([char]9492).ToString()
    'BottomRight' = ([char]9496).ToString()
    'Cross' = ([char]9532).ToString()
    'VerticalDoubleRightSingle' = ([char]9567).ToString()
    'VerticalDoubleLeftSingle' = ([char]9570).ToString()
    'HorizontalDoubleSingleUp' = ([char]9575).ToString()
}
 
$box = new-object object
 
$BoxChars.GetEnumerator() |
  Foreach {
    Add-Member -InputObject $box -Name $_.name -MemberType NoteProperty -Value $_.value
  }
  If ($DoubleBorder) {
  $LineTop = $box.TopLeftDouble `
    + $box.HorizontalDouble * ($size.width - 2) `
    + $box.TopRightDouble
  $LineField = $box.VerticalDouble `
    + ' ' * ($size.width - 2) `
    + $box.VerticalDouble
  $LineBottom = $box.BottomLeftDouble `
    + $box.HorizontalDouble * ($size.width - 2) `
    + $box.BottomRightDouble
  } else {  
  $LineTop = $box.TopLeft `
    + $box.Horizontal * ($size.width - 2) `
    + $box.TopRight
  $LineField = $box.Vertical `
    + ' ' * ($size.width - 2) `
    + $box.Vertical
  $LineBottom = $box.BottomLeft `
    + $box.Horizontal * ($size.width - 2) `
    + $box.BottomRight
 }
    $box = &{$LineTop;1..($size.Height - 2) |% {$LineField};$LineBottom}
    $boxBuffer = $host.ui.rawui.NewBufferCellArray($box,$ForegroundColor,$BackgroundColor)
    ,$boxBuffer
}

Function Get-ContentSize ($content){
    $max = $content |% {([string]$_).length} | Measure-Object -Maximum |% {$_.maximum}
    [Drawing.Size]"$($max),$($content.Length)"
}

function get-Position ($X,$Y) {
  $WindowPosition  = $host.ui.rawui.windowposition 
  $Position = $WindowPosition
  $Position.X += $X
  $Position.Y += $Y
  ,$Position
}
Function Place-Buffer ($Position,$buffer) {
  $bufferTop = $Position
  $bufferBottom = $bufferTop
  $bufferBottom.X += ($buffer.GetUpperBound(1) )
  $bufferBottom.Y += ($buffer.GetUpperBound(0) )
  $Rectangle = New-Object management.automation.host.rectangle( $bufferTop , $bufferBottom)
  $OldBuffer = $Host.UI.RawUI.GetBufferContents($rectangle)
  $host.ui.rawui.SetBufferContents($bufferTop,$buffer)
  $Handle = new-object object
  Add-Member -InputObject $Handle -MemberType NoteProperty -Name Content -value $buffer
  Add-Member -InputObject $Handle -MemberType NoteProperty -Name OldContent -value $OldBuffer
  Add-Member -InputObject $Handle -MemberType NoteProperty -Name Location -value $BufferTop
  Add-Member -InputObject $Handle -MemberType ScriptMethod -Name clear -value {$host.ui.rawui.SetBufferContents($this.Location,$this.OldContent)}
  Add-Member -InputObject $Handle -MemberType ScriptMethod -Name Show -value {$host.ui.rawui.SetBufferContents($this.Location,$this.Content)}
  return $Handle
}

Function ConvertTo-BufferCellArray (
  $Content,
  $ForegroundColor = $Host.UI.RawUI.ForegroundColor,
  $BackgroundColor = $Host.UI.RawUI.BackgroundColor
){
  $content = $content |% {$_}
  ,$host.ui.rawui.NewBufferCellArray($content,$ForegroundColor,$BackgroundColor)
}


Function parse-List ($Size) {

  $WindowPosition  = $host.UI.rawui.windowposition
  $WindowSize = $Host.UI.RawUI.WindowSize
  $Cursor = $host.UI.RawUI.CursorPosition
  $Center = [math]::Truncate($WindowSize.Height / 2)
  $CursorOffset = $Cursor.Y - $WindowPosition.Y
  $CursorOffsetBottom = $WindowSize.Height - $CursorOffset

  # Vertical Placement and size

  $ListHeight = $Size.Height + 2

  If ( ($CursorOffset -gt $center) -and ($ListHeight -ge $CursorOffsetBottom)  ) {$Placement = 'Above'}
  else {$Placement =  'Below'}

  Switch ($placement) {
      'Above' {
          $MaxListHeight =  $CursorOffset 
          if ($MaxListHeight -lt $ListHeight) {$listHeight = $MaxListHeight}
          $Y = $CursorOffset - $listHeight
      }
      'Below' {
          $MaxListHeight = ($CursorOffsetBottom - 1)  
          if ($MaxListHeight -lt $ListHeight) {$listHeight = $MaxListHeight}
          $Y = $CursorOffSet + 1
      }
  }
  $MaxItems = $MaxListHeight -2

  # Horizontal

  $ListWidth = $size.Width + 4
    if ($ListWidth -gt $WindowSize.width) {$ListWidth = $Windowsize.width  }
    if ($ListWidth -lt 18) {$ListWidth = 18 }
    $Max = $ListWidth 
    if ( ($Cursor.X + $max) -lt ($WindowSize.Width -2 ) ) {
            $X = $Cursor.X
    } else {        
        if (($Cursor.X - $max ) -gt 0) {
            $X = ($Cursor.X - $max )
        } else {
            $X = $windowSize.Width - $max
        }
    }

  #output

  $ListInfo = new-object object
  Add-Member -InputObject $ListInfo -MemberType NoteProperty -Name Orientation -value $Placement
  Add-Member -InputObject $ListInfo -MemberType NoteProperty -Name TopX -value $X
  Add-Member -InputObject $ListInfo -MemberType NoteProperty -Name TopY -value $Y
  Add-Member -InputObject $ListInfo -MemberType NoteProperty -Name ListHeight -Value $ListHeight
  Add-Member -InputObject $ListInfo -MemberType NoteProperty -Name ListWidth -Value $ListWidth
  Add-Member -InputObject $ListInfo -MemberType NoteProperty -Name MaxItems -Value $MaxItems
  $ListInfo
}

Function Invoke-ConsoleList  ($content,$Borderfgc,$Borderbgc,$Contentfgc,$ContentBgc){

  $size = Get-ContentSize $content
  if ($Size.Width -lt 16) {$Size.Width = 16}
  $content = $content |% {" $_ ".padright($Size.Width + 2)}
  $ListConfig = parse-List $size
  $BoxSize = New-Object drawing.size( $ListConfig.ListWidth  , $ListConfig.ListHeight  )
  $box = new-Box $BoxSize $Borderfgc $Borderbgc

  $Position = get-Position $ListConfig.TopX $ListConfig.topY
  $BoxHandle = Place-Buffer $position $box

  # Place content 

  $Position.X += 1
  $position.Y += 1
  $ContentBuffer = ConvertTo-BufferCellArray @($content)[0..($Listconfig.ListHeight -3 )] $Contentfgc $ContentBgc
  $ContentHandle = Place-Buffer $position $contentBuffer
  $Handle = new-object object
  Add-Member -InputObject $Handle -MemberType NoteProperty -Name Position -value (get-Position $ListConfig.TopX $ListConfig.topY)
  Add-Member -InputObject $Handle -MemberType NoteProperty -Name ListConfig -value $ListConfig
  Add-Member -InputObject $Handle -MemberType NoteProperty -Name ContentSize -value $size
  Add-Member -InputObject $Handle -MemberType NoteProperty -Name BoxSize -value $BoxSize
  Add-Member -InputObject $Handle -MemberType NoteProperty -Name Box -value $BoxHandle
  Add-Member -InputObject $Handle -MemberType NoteProperty -Name Content -value $ContentHandle
  Add-Member -InputObject $Handle -MemberType NoteProperty -Name SelectedItem -value 0
  Add-Member -InputObject $Handle -MemberType NoteProperty -Name SelectedLine -value 1
  Add-Member -InputObject $Handle -MemberType NoteProperty -Name Items -value $Content
  Add-Member -InputObject $Handle -MemberType NoteProperty -Name FirstItem -value 0
  Add-Member -InputObject $Handle -MemberType NoteProperty -Name LastItem -value ($Listconfig.ListHeight -3 )
  Add-Member -InputObject $Handle -MemberType NoteProperty -Name MaxItems -value $Listconfig.MaxItems
  Add-Member -InputObject $Handle -MemberType ScriptMethod -Name clear -value {$this.box.clear()}
  Add-Member -InputObject $Handle -MemberType ScriptMethod -Name Show -value {
    $this.box.Show()
    $this.content.Show()
  }
  return $handle
}

    Function Write-Line ($x,$y,[string]$Text ,[system.consolecolor]$fgc,[system.consolecolor]$bgc) {
        $pos = $host.ui.RawUI.WindowPosition
        $pos.x += $x
        $pos.y += $y
        if ($text -eq '') {$text = '-'}
        $row = $host.ui.rawui.NewBufferCellArray($text,$fgc,$bgc) 
        $host.ui.rawui.SetBufferContents($pos,$row) 
    } 

    Function SelectLine ($x,$y,$w,[system.consolecolor]$fgc,[system.consolecolor]$bgc) {
        $pos = $ListHandle.position
        $pos.x += $x
        $pos.y += $y
        $rect = "system.management.automation.host.rectangle" 
        $LineRect = new-object $rect $pos.x,$pos.y,($pos.x + $w),($pos.y) 
        $LineBuffer = $host.ui.rawui.getbuffercontents($LineRect) 
        $LineBuffer = $host.ui.rawui.NewBufferCellArray(@([string]::join("",($LineBuffer|%{$_.character}))),$fgc,$bgc)
        $host.ui.rawui.SetBufferContents($Pos,$LineBuffer)
    }
    Function Move-List ($x,$y,$Width,$Height,$Offset){
        $pos = $ListHandle.position
        $rect = "system.management.automation.host.rectangle"
        $pos.Y += $Y
        $pos.X += $X
        $re = new-object $rect $pos.x,$pos.y,($pos.x + $width),($pos.y + $height -1)
        $pos.Y += $OffSet
        $bc = New-Object System.Management.Automation.Host.BufferCell
        $bc.BackgroundColor = $PowerTabConfig.Colors.BackColor
        $Host.UI.RawUI.ScrollBufferContents($re,$pos,$re,$bc)

    }

Function Move-selection ($count,$noScroll = $false){
    $SelectedItem = $ListHandle.SelectedItem
    $line = $listhandle.SelectedLine
    if ($count -eq ([math]::abs($count))) {# Down in list
        if ($SelectedItem -eq (@($ListHandle.Items).count -1)) {Return}
        $one = 1
        if ($SelectedItem -eq $ListHandle.lastItem ) {
            $Move = $true
            if (($ListHandle.Items.count - $SelectedItem -1 ) -lt $count) {$count = ($ListHandle.Items.count - $SelectedItem - 1)}
        } else {
            $Move = $False
            if (($ListHandle.MaxItems - $Line ) -lt $count) {$count = ($ListHandle.MaxItems - $line)}       
        }

    } else {
        if ($SelectedItem -eq 0) {Return}
        $one = -1
        if ($SelectedItem -eq $ListHandle.FirstItem  ) {
             $Move = $true
             if ($SelectedItem -lt ([math]::abs($count))) {$count = (-($SelectedItem))}
        } else {
           $Move = $False
           if ($line -lt ([math]::abs($count))) {$count = (-$line) + 1}
        }

    }

    if ($move) {
        SelectLine 1 ($line) ($listHandle.ListConfig.ListWidth -3 )$TextColor $BackColor
        if (-not $noScroll) {Move-List 1 1 ($listHandle.ListConfig.ListWidth -3 ) ($listHandle.ListConfig.ListHeight - 2 ) (-$count)}
        # Move-List 1 1 ($listHandle.ListConfig.ListWidth -3 ) ($listHandle.ListConfig.ListHeight - 2 ) (-$count)
        $SelectedItem += $count
        $ListHandle.FirstItem += $count
        $ListHandle.LastItem += $count  
                      
        
        $LinePosition = $ListHandle.position
        $LinePosition.X +=1
        if($one -eq 1){
            $LinePosition.Y += $line - ($count - $one)
            $LineBuffer =  ConvertTo-BufferCellArray ($ListHandle.Items[($SelectedItem - ($count - $one)) .. $SelectedItem]) $TextColor $BackColor
        } else {
            $LinePosition.Y += 1
            $LineBuffer =  ConvertTo-BufferCellArray ($ListHandle.Items[($SelectedItem..($selectedItem - ($count - $one)))]) $TextColor $BackColor
        }
        $LineHandle = Place-Buffer $LinePosition $LineBuffer
        SelectLine 1 ($line) ($listHandle.ListConfig.ListWidth -3 )$SelectedTextColor $SelectedBackColor
    } Else {
        SelectLine 1 ($Line) ($listHandle.ListConfig.ListWidth -3 )$TextColor $BackColor 
        $SelectedItem += $count
        $Line += $count
        SelectLine 1 ($Line) ($listHandle.ListConfig.ListWidth -3 )$SelectedTextColor $SelectedBackColor
    }
    $ListHandle.SelectedItem = $SelectedItem
    $ListHandle.SelectedLine = $Line
    $StatusHandle.clear()
    $StatusBuffer =  ConvertTo-BufferCellArray "[$($listHandle.SelectedItem + 1)] $($listHandle.FirstItem + 1)-$($listHandle.LastItem + 1) [$($Content.Length)]" $BorderTextColor $BorderBackColor
    $StatusHandle = Place-Buffer $StatusHandle.Location $StatusBuffer
}

#### Main Starts here ####


    # Place inital list
        $filter = ''
        $ListHandle = Invoke-ConsoleList $content $BorderColor $BorderBackColor $TextColor $BackColor

        $previewBuffer =  ConvertTo-BufferCellArray "$filter " $FilterColor $Host.UI.RawUI.BackgroundColor
        $preview = Place-Buffer $host.UI.RawUI.CursorPosition $previewBuffer

    Function add-Status {   

        $TitleBuffer =  ConvertTo-BufferCellArray " $LastWord" $BorderTextColor $BorderBackColor
        $TitlePosition = $ListHandle.position
        $TitlePosition.X +=2
        $TitleHandle = Place-Buffer $TitlePosition $TitleBuffer
    
        $FilterBuffer =  ConvertTo-BufferCellArray "$filter " $FilterColor $BorderBackColor
        $FilterPosition = $ListHandle.position
        $FilterPosition.X += ( 3 + $lastWord.length )
        $FilterHandle = Place-Buffer $FilterPosition $filterBuffer

        $StatusBuffer =  ConvertTo-BufferCellArray "[$($listHandle.SelectedItem + 1)] $($listHandle.FirstItem + 1)-$($listHandle.LastItem + 1) ($($listHandle.items.count)/$($Content.Length))]" $BorderTextColor $BorderBackColor
        $StatusPosition = $ListHandle.position
        $StatusPosition.X += 2
        $StatusPosition.Y += ($listHandle.ListConfig.ListHeight -1 )
        $StatusHandle = Place-Buffer $StatusPosition $StatusBuffer

    }
    . add-Status 

   # handle selection


   $SelectedItem = 0
   SelectLine 1 ($SelectedItem + 1) ($listHandle.ListConfig.ListWidth -3 )$SelectedTextColor $SelectedBackColor

    # Ask for key and start loop

    $Key = $host.ui.RawUI.ReadKey('NoEcho,IncludeKeyDown')
    $Continue = $true

    # Process key's

    $filter = ''
    while ( $key.VirtualKeyCode -ne 27 -and $Continue -eq $true) {
    if (-not $HasChild) {
        if ($oldFilter -ne $filter) { 
          $preview.clear()
          $previewBuffer =  ConvertTo-BufferCellArray "$filter " $FilterColor $Host.UI.RawUI.BackgroundColor
          $preview = Place-Buffer $preview.Location $previewBuffer
        }
        $oldFilter = $filter
}
        $Shift = $key.ControlKeyState.tostring()
        $HasChild = $False
        Switch ($key.VirtualKeyCode){
            9 { # Tab
                if ($shift -match 'ShiftPressed') { # Up
                  Move-Selection 1 
                }else { # down
                  Move-Selection -1 
                }
                break
            }
            38 { # Up Arrow
                if ($shift -match 'ShiftPressed') { 
                  if ($FastScrollItemCount -gt ($ListHandle.Items.count -1)){$count = ($ListHandle.Items.count -1)}else{$count = $FastScrollItemCount}
                  Move-Selection (- $count) 
                }else { # down
                  Move-Selection -1 
                }
                break
            }
            40 { # Down Arrow
                if ($shift -match 'ShiftPressed') {
                  if ($FastScrollItemCount -gt ($ListHandle.Items.count -1)){$count = ($ListHandle.Items.count -1)}else{$count = $FastScrollItemCount}
                  Move-Selection $count 
                }else { 
                  Move-Selection 1 
                }
                break
            }

            33 { #PageUp
                $count = $listHandle.items.count
                if ($count -gt $ListHandle.maxItems) {$count = $ListHandle.maxItems}
                Move-Selection (-($count -1)) 
                break
            }
            34 { #PageDown
                $count = $listHandle.items.count
                if ($count -gt $ListHandle.maxItems) {$count = $ListHandle.maxItems}
                Move-Selection ($count -1) 
                break
            }
            39 { # right arrow
                $char = (@($ListHandle.Items)[$listHandle.SelectedItem ][($lastword.length + $filter.length +1)])
                $filter+= $char
                $Old = $items.Length
                $items = $Content -match ([regex]::Escape("$lastword$Filter") +'.*')
                $new = $items.Length
                if ($items.Length -lt 1) {
                    Write-Host -no "`a"
                    $filter=$filter.substring(0,$filter.length-1)
                } Else {
                    if ($old -ne $new) {
                      $ListHandle.clear()
                      $ListHandle = Invoke-ConsoleList $Items $BorderColor $BorderBackColor $TextColor $BackColor
                      . add-Status
                    } 
                    $SelectedItem = 0
                    SelectLine 1 ($SelectedItem + 1) ($listHandle.ListConfig.ListWidth -3 )$SelectedTextColor $SelectedBackColor
                    $host.ui.write($FilterColor,$Host.UI.RawUI.BackgroundColor,$char)
                }
                break
            }
            {(8,37 -contains $_)} { # Backspace or left arrow
                if ($filter)
                {
                    $filter=$filter.substring(0,$filter.length-1)
                    $host.ui.write([char]8)
                    Write-Line ($host.UI.RawUI.CursorPosition.x) ($host.UI.RawUI.CursorPosition.y-$host.UI.RawUI.WindowPosition.y) " " $FilterColor $Host.UI.RawUI.BackgroundColor
                    $Old = $items.Length
                    $items = $Content -match ([regex]::Escape("$lastword$Filter") +'.*')
                    $new = $items.Length
                    if ($old -ne $new) {
                      $ListHandle.clear()
                      $ListHandle = Invoke-ConsoleList $Items $BorderColor $BorderBackColor $TextColor $BackColor
                      . add-Status
                    }  
                       $SelectedItem = 0
                       SelectLine 1 ($SelectedItem + 1) ($listHandle.ListConfig.ListWidth -3 )$SelectedTextColor $SelectedBackColor
                }
                else
                {
                    if ($CloseListOnEmptyFilter) {
                        $key.VirtualKeyCode = 27
                        $Continue = $False
                    } else {
                        Write-Host -no "`a"
                    }
                }
                break
            }
            190 { #Dot
                if ($DotComplete) {
                if ($AutoExpandOnDot) {
                    $host.ui.write($Host.UI.RawUI.ForegroundColor,$Host.UI.RawUI.BackgroundColor,(@($ListHandle.Items)[$listHandle.SelectedItem ].trim().Substring($lastword.length + $filter.length) + '.'))
                    $ListHandle.clear()
                    $linePart = $line.substring(0,$line.Length - $lastword.Length)

  if ($script:MessageHandle){$host.ui.rawui.SetBufferContents($MessageHandle.Top,$MessageHandle.Buffer)
    remove-Variable -Name MessageHandle -Scope script
  }
                    . Tabexpansion ($linePart + @($ListHandle.Items)[$listHandle.SelectedItem ].trim() + '.') (@($ListHandle.Items)[$listHandle.SelectedItem ].trim() + '.') -ForceList
                    $HasChild = $True
                } else {
                    @($ListHandle.Items)[$listHandle.SelectedItem ].trim()
                }
                $Continue = $False
                break
                }
            }
            {($key.Character -eq '\') -or ($key.Character -eq '/')} { # Path Separators
                if ($BackSlashComplete) {
                if ($AutoExpandOnBackSlash) {
                    $host.ui.write($Host.UI.RawUI.ForegroundColor,$Host.UI.RawUI.BackgroundColor,(@($ListHandle.Items)[$listHandle.SelectedItem ].trim().Substring($lastword.length + $filter.length) + '\'))
                    $ListHandle.clear()
                    if ($line.Length -ge $lastword.Length){$linePart = $line.substring(0,$line.Length - $lastword.Length)}

  if ($script:MessageHandle){$host.ui.rawui.SetBufferContents($MessageHandle.Top,$MessageHandle.Buffer)
    remove-Variable -Name MessageHandle -Scope script
  }
                    . Tabexpansion ($linePart + @($ListHandle.Items)[$listHandle.SelectedItem ].trim() + '\') (@($ListHandle.Items)[$listHandle.SelectedItem ].trim() + '\')
                    $HasChild = $True
                } else {
                    @($ListHandle.Items)[$listHandle.SelectedItem ].trim()
                }
                $Continue = $False
                break
                }
            }
            32 { #Space
                if (($SpaceComplete -and -not ($key.ControlKeyState -match 'CtrlPressed')) -or (-not $SpaceComplete -and ($key.ControlKeyState -match 'CtrlPressed')) ) {
                $item = @($ListHandle.Items)[$listHandle.SelectedItem ].trim()
                if ((-not $item.Contains(' ')) -and ($script:PowerTabfileSystemMode -ne $true)) {$item += ' '}
                $item
                $Continue = $False
                break
                }
            }
            {($CustomCompletionChars.toCharArray() -contains $key.Character) -and $CustomComplete} { # Extra completions
                if ($true) {
                $item = @($ListHandle.Items)[$listHandle.SelectedItem ].trim()
                $item = ($item + $key.Character) -replace "\$($key.character){2}$" , $key.character
                $item
                $Continue = $False
                break
                }
            }
            13 { #Enter
                @($ListHandle.Items)[$listHandle.SelectedItem ].trim()
                $Continue = $False
                break
            }
            {$_ -ge 32 -and $_ -le 190}  { #Char or digit or symbol
                $filter+=$key.character
                $Old = $items.Length
                $items = $Content -match ([regex]::Escape("$lastword$Filter") +'.*')
                $new = $items.Length
                if ($items.Length -lt 1) {
                    if ($CloseListOnEmptyFilter) {
                      $ListHandle.clear()
                      return "$ReturnWord$filter"
                     } else {
                      Write-Host -no "`a"
                      $filter=$filter.substring(0,$filter.length-1)
                     }
                } Else {
                    if ($old -ne $new) {
                      $ListHandle.clear()
                      $ListHandle = Invoke-ConsoleList $Items $BorderColor $BorderBackColor $TextColor $BackColor
                      . add-Status
                   $SelectedItem = 0
                   SelectLine 1 ($SelectedItem + 1) ($listHandle.ListConfig.ListWidth -3 )$SelectedTextColor $SelectedBackColor
                    } 

                $host.ui.write($FilterColor,$Host.UI.RawUI.BackgroundColor,$key.character)
                }
                break
            }
        }
        #$Host.UI.RawUI.FlushInputBuffer()
        If ($Continue) {$Key = $host.ui.RawUI.ReadKey('NoEcho,IncludeKeyDown')}
    }
    $listHandle.clear()
    if (-not $HasChild){
    if ($key.VirtualKeyCode -eq 27) {
		#WriteLine ($host.UI.RawUI.CursorPosition.x -1 ) ($host.UI.RawUI.CursorPosition.y-$host.UI.RawUI.WindowPosition.y) " " $FilterColor $Host.UI.RawUI.BackgroundColor
        Return "$ReturnWord$filter"
    }
    }
}



# gcm get-* |% {$_.name} | . Out-ConsoleList get-