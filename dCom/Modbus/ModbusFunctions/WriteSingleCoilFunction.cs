using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write coil functions/requests.
    /// </summary>
    public class WriteSingleCoilFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleCoilFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleCoilFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusWriteCommandParameters p = CommandParameters as ModbusWriteCommandParameters;
            byte[] packet = new byte[12];

            packet[0] = (byte)(p.TransactionId >> 8);
            packet[1] = (byte)(p.TransactionId & 0xFF);
            packet[2] = 0;
            packet[3] = 0;
            packet[4] = 0;
            packet[5] = 6;
            packet[6] = p.UnitId;
            packet[7] = p.FunctionCode;
            packet[8] = (byte)(p.OutputAddress >> 8);
            packet[9] = (byte)(p.OutputAddress & 0xFF);
            // Modbus coil: 0xFF00 = ON, 0x0000 = OFF
            ushort coilValue = p.Value != 0 ? (ushort)0xFF00 : (ushort)0x0000;
            packet[10] = (byte)(coilValue >> 8);
            packet[11] = (byte)(coilValue & 0xFF);

            return packet;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            var result = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if ((response[7] & 0x80) != 0)
            {
                HandeException(response[8]);
            }

            ushort address = (ushort)((response[8] << 8) | response[9]);
            ushort rawValue = (ushort)((response[10] << 8) | response[11]);
            ushort coilValue = rawValue != 0 ? (ushort)1 : (ushort)0;
            result[new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, address)] = coilValue;

            return result;
        }
    }
}