﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Cognitivo.Class
{
    public static class CustomCommands
    {
       // static Cognitivo.GestureSettings propGesture = new Cognitivo.GestureSettings();

        //Used For Delete Operation in Child DataGrid.
        public static readonly RoutedUICommand Delete =
            new RoutedUICommand("Delete", "Delete", typeof(CustomCommands)); //, new InputGestureCollection() { new KeyGesture(Key.Delete, ModifierKeys.Alt) }

       

    }
}
