using EasyModbus;
using LogicModule.ObjectModel.TypeSystem;
using System;

namespace LogicNodesModbusHelper
{
    public static class LogicNodesModbus
    {
        public static class FunctionCodeEnum
        {
            public const string FC_03 = "Read Holding Registers (03)";
            public const string FC_04 = "Read Input Registers (04)";
            public static string[] VALUES = new[] { FC_03, FC_04 };
        }
        public static class DataTypeEnum
        {
            public const string INT16_UNSIGNED = "16bit integer";
            public const string INT16_SIGNED = "16bit integer (signed)";
            public const string INT32 = "integer (32bit)";
            public const string FLOAT = "float (32bit)";
            public const string LONG = "long (64bit)";
            public const string DOUBLE = "double (64bit)";

            public static string[] VALUES = new[] { INT16_SIGNED, INT16_UNSIGNED, INT32, FLOAT, LONG, DOUBLE };
        }

        public static class ByteOrderEnum
        {
            public const string HIGH_LOW = "big-endian";
            public const string LOW_HIGH = "little-endian";

            public static string[] VALUES = new[] { HIGH_LOW, LOW_HIGH };
        }
        public static void writeRegister(ModbusClient modbusClient, int register, int value, String dataType, ref string errorMessage)
        {
            try
            {
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
                        errorMessage = "INTERNAL: unsupported datatype";
                        break;
                }
                errorMessage += $"Write {register}:{value} done;";
            }
            catch (Exception e)
            {
                errorMessage += e.ToString() + ";";
            }
        }

        public static int readRegister(ModbusClient modbusClient, int startRegister, String dataType, ref string errorMessage, string functionCode = FunctionCodeEnum.FC_03)
        {
            ModbusClient.RegisterOrder regOrder;
            regOrder = ModbusClient.RegisterOrder.HighLow;

            int registerToRead;
            switch (dataType)
            {
                case DataTypeEnum.INT32:
                case DataTypeEnum.FLOAT:
                    registerToRead = 2;
                    break;
                case DataTypeEnum.LONG:
                case DataTypeEnum.DOUBLE:
                    registerToRead = 4;
                    break;
                case DataTypeEnum.INT16_SIGNED:
                case DataTypeEnum.INT16_UNSIGNED:
                default:
                    registerToRead = 1;
                    break;
            }

            int[] readRegisters;
            int retry = 5;

            while (true)
                try
                {
                    switch(functionCode)
                    {
                        case FunctionCodeEnum.FC_04:
                            readRegisters = modbusClient.ReadInputRegisters(startRegister, registerToRead);
                            break;
                        case FunctionCodeEnum.FC_03:
                        default:
                            readRegisters = modbusClient.ReadHoldingRegisters(startRegister, registerToRead);
                            break;

                    }
                    if (functionCode == FunctionCodeEnum.FC_03)
                    {
                        
                    }
                    break;
                }
                catch (System.IO.IOException e)
                {
                    retry--;
                    if (retry == 0)
                    {
                        return -1;
                    }
                    System.Threading.Thread.Sleep(500);
                }

            double result = 0;
            string result_str = "";

            switch (dataType)
            {
                case DataTypeEnum.INT32:
                    result = ModbusClient.ConvertRegistersToInt(readRegisters, regOrder);
                    break;
                case DataTypeEnum.FLOAT:
                    result = ModbusClient.ConvertRegistersToFloat(readRegisters, regOrder);
                    break;
                case DataTypeEnum.LONG:
                    result = ModbusClient.ConvertRegistersToLong(readRegisters, regOrder);
                    break;
                case DataTypeEnum.DOUBLE:
                    result = ModbusClient.ConvertRegistersToDouble(readRegisters, regOrder);
                    break;
                case DataTypeEnum.INT16_SIGNED:
                    result = readRegisters[0];
                    break;
                case DataTypeEnum.INT16_UNSIGNED:
                    // unsigned
                    for (int i = 0; i < (readRegisters.Length); i++)
                    {
                        int tmp = readRegisters[i];
                        if (tmp == -32768) // fix for 0x00
                            tmp = 0;
                        if (tmp < 0) // no negative values !
                            tmp = tmp + (int)Math.Pow(2, 16);
                        result = result + (tmp * Math.Pow(2, (16 * ((readRegisters.Length) - (i + 1)))));
                        result_str = result_str + " 0x" + tmp.ToString("X4");
                    }
                    break;
                default:
                    result_str = "internal: invalid datatype";
                    break;
            }

            errorMessage += result_str + ";";
            return (int)result;
        }
    }
}
