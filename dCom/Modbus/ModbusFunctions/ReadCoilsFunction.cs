using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read coil functions/requests.
    /// </summary>
    public class ReadCoilsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadCoilsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
		public ReadCoilsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc/>
        public override byte[] PackRequest()
        {
            //TO DO: IMPLEMENT
            byte[] retArray = new byte[12];

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)CommandParameters.TransactionId)), 0, retArray, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)CommandParameters.ProtocolId)), 0, retArray, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)CommandParameters.Length)), 0, retArray, 4, 2);

            retArray[6] = CommandParameters.UnitId;
            retArray[7] = CommandParameters.FunctionCode;

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)((ModbusReadCommandParameters)CommandParameters).StartAddress)), 0, retArray, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)((ModbusReadCommandParameters)CommandParameters).Quantity)), 0, retArray, 10, 2);

            return retArray;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            //TO DO: IMPLEMENT
            Dictionary<Tuple<PointType, ushort>, ushort> ret = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if ((response[7] & 0x80) != 0)
            {
                HandeException(response[8]);
                return ret;
            }

            int bytCnt = response[8];
            int rest = ((ModbusReadCommandParameters)CommandParameters).Quantity;
            ushort addr = ((ModbusReadCommandParameters)CommandParameters).StartAddress;

            int bytIndx = 0;

            while(bytIndx < bytCnt && rest > 0) 
            {
                byte currByt = response[9 + bytIndx];

                int bitIndx = 0;
                while(bitIndx < 8 && rest > 0) 
                {
                    ushort value = (ushort)(currByt & 0x01);

                    var key = new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT,addr);

                    ret[key] = value;

                    currByt >>= 1;
                    addr++;
                    bitIndx++;
                    rest--;
                }

                bytIndx++;
            }

            return ret;
        }
    }
}