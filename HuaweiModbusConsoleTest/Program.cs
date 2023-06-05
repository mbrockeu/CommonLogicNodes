// See https://aka.ms/new-console-template for more information

using EasyModbus;
using marcus_brock_mbrock_eu.logic.HuaweiModbus;

Console.WriteLine("Hello, World!");


    ModbusClient modbusClient = null;
    try
    {
        modbusClient = new ModbusClient("192.168.1.58", 502);
        modbusClient.ConnectionTimeout = 5000;
        modbusClient.Connect();
        modbusClient.UnitIdentifier = 1;

    var register = 47086;
    var dataType = DataTypeEnum.INT16_UNSIGNED;
    var value = 2;
        // needed?
        System.Threading.Thread.Sleep(700);
        switch (dataType)
        {
            case DataTypeEnum.INT32:
                int[] toWrite = ModbusClient.ConvertIntToRegisters(value, ModbusClient.RegisterOrder.HighLow);
                modbusClient.WriteMultipleRegisters(register, toWrite);
                break;
            case DataTypeEnum.INT16_UNSIGNED:
                modbusClient.WriteSingleRegister(register, value);
                break;
            default:
                Console.WriteLine("INTERNAL: unsupported datatype");
                break;
        }
    }
    catch (Exception e)
    {
    Console.WriteLine(e.ToString());
    }
    finally
    {
        if (modbusClient != null)
        {
            modbusClient.Disconnect();
        }
    }

Console.ReadLine();