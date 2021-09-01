﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ikrito_Fulfillment_Platform.Utils;
using Ikrito_Fulfillment_Platform.Modules;
using Newtonsoft.Json;

namespace Ikrito_Fulfillment_Platform {
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page {
        public MainPage() {
            InitializeComponent();

            OrderGetter orderGetter = new();

            dynamic orders = JsonConvert.DeserializeObject(orderGetter.getOrders());

            testLabel.Content = orders["orders"];
            

        }
    }
}
