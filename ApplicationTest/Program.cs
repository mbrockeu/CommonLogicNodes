using EasyModbus;
using LogicModule.ObjectModel.TypeSystem;
using LogicModule.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LogicNodesModbusHelper.LogicNodesModbus;
using System.Runtime.Remoting.Contexts;
using LogicModule.Nodes.Helpers;
using System.Runtime.Remoting.Messaging;

namespace ApplicationTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var errorMessage = string.Empty;
            var modbusClient = new ModbusClient("192.168.1.58", 502);
            modbusClient.ConnectionTimeout = 8000;
            modbusClient.Connect();
            modbusClient.UnitIdentifier = 1;
            //var value = readRegister(modbusClient, 5, DataTypeEnum.INT32, ref FunctionCodeEnum.FC_04);
            writeRegister(modbusClient, 47075, 2500, DataTypeEnum.INT32, ref errorMessage);
            modbusClient.Disconnect();

            Console.ReadLine();
        }
    }
}
