using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read holding registers functions/requests.
    /// </summary>
    public class ReadHoldingRegistersFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadHoldingRegistersFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadHoldingRegistersFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusReadCommandParameters p = CommandParameters as ModbusReadCommandParameters;
            byte[] packet = new byte[12];

            packet[0] = (byte)(p.TransactionId >> 8);
            packet[1] = (byte)(p.TransactionId & 0xFF);
            packet[2] = 0;
            packet[3] = 0;
            packet[4] = 0;
            packet[5] = 6;
            packet[6] = p.UnitId;
            packet[7] = p.FunctionCode;
            packet[8] = (byte)(p.StartAddress >> 8);
            packet[9] = (byte)(p.StartAddress & 0xFF);
            packet[10] = (byte)(p.Quantity >> 8);
            packet[11] = (byte)(p.Quantity & 0xFF);

            return packet;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            ModbusReadCommandParameters p = CommandParameters as ModbusReadCommandParameters;
            var result = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if ((response[7] & 0x80) != 0)
            {
                HandeException(response[8]);
            }

            for (int i = 0; i < p.Quantity; i++)
            {
                ushort value = (ushort)((response[9 + i * 2] << 8) | response[10 + i * 2]);
                result[new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, (ushort)(p.StartAddress + i))] = value;
            }

            return result;
        }
    }
}