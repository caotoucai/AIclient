using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAN_Library.CAN_Parameter
{
    public interface ICAN_Signal: ISignal, Rolling, E2E
    {
        double Factor { set; get; }
        double Offset { set; get; }
        double Min_Value { set; get; }
        double Max_Value { set; get; }
        int ByteOrder { set; get; }
        int ValueType { set; get; }

        double olderValue { set; get; }

        List<CANSignalChangeInfo> valueChangeList { set; get; }
        List<SignalValueRecord> valueRecordList { set; get; }
    }

    public interface ILIN_Signal : ISignal
    {
    }

    /// <summary>
    /// base
    /// </summary>
    public interface Rolling
    {
        bool isRolling { set; get; }
        double Rolling_Step { set; get; }
        double Rolling_MaxValue { set; get; }
        double Rolling_MinValue { set; get; }
    }

    public interface E2E
    {
        bool is_E2E_Checksum { set; get; }
        bool is_E2E_RollingCount { set; get; }
        bool is_E2E_ReservedBit_Fill_One { set; get; }//true Fill 1, false fill 0
        bool is_E2E_DataID_CRC_Done { set; get; }

        int E2E_StartByte { set; get; }
        int E2E_EndByte { set; get; }
        int E2E_Method { set; get; }
        byte E2E_DataID_HByte { set; get; }
        byte E2E_DataID_LByte { set; get; }

        byte E2E_DataID_CRC_Value { set; get; }
    }

    public interface ISignal: INotifyPropertyChanged
    {
        string Name { set; get; }
        int StartBit { set; get; }
        int Length { set; get; }
        double Init_Value { set; get; }
        double Maximum { set; get; }
        double Minimum { set; get; }
        string Unit { set; get; }
        double Value { set; get; }
    }


}
