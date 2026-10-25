/*-------------------------------------------------------------------------------------------
| File        : FlexRayConfig.cs
| Project     : Vector FlexRay Fibex to local data converter example
|
| Description : Class which provides the cluster configuration data storage
|--------------------------------------------------------------------------------------------
| $Author: vismra $    $Date: 2009-12-18 10:49:31 +0100 (Fri, 18 Dec 2009) $   $Revision: 4562 $
| $Id: FlexRayConfig.cs 4562 2009-12-18 09:49:31Z vismra $
|--------------------------------------------------------------------------------------------
| Copyright (c) 2009 by Vector Informatik GmbH.  All rights reserved.
|------------------------------------------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Reflection;

namespace Fibex_Parser
{
  //--------------------------------------------------------------------------------------------------------
  /// <summary>
  /// This class represents a XLAPI-styled Flexray cluster configuratino
  /// </summary>
  //--------------------------------------------------------------------------------------------------------
  public class FlexRayConfiguration
  {
    public uint busGuardianEnable;
    public uint baudrate;
    public uint busGuardianTick;
    public uint externalClockCorrectionMode;
    public uint gColdStartAttempts;
    public uint gListenNoise;
    public uint gMacroPerCycle;
    public uint gMaxWithoutClockCorrectionFatal;
    public uint gMaxWithoutClockCorrectionPassive;
    public uint gNetworkManagementVectorLength;
    public uint gNumberOfMinislots;
    public uint gNumberOfStaticSlots;
    public uint gOffsetCorrectionStart;
    public uint gPayloadLengthStatic;
    public uint gSyncNodeMax;
    public uint gdActionPointOffset;
    public uint gdDynamicSlotIdlePhase;
    public uint gdMacrotick;
    public uint gdMinislot;
    public uint gdMiniSlotActionPointOffset;
    public uint gdNIT;
    public uint gdStaticSlot;
    public uint gdSymbolWindow;
    public uint gdTSSTransmitter;
    public uint gdWakeupSymbolRxIdle;
    public uint gdWakeupSymbolRxLow;
    public uint gdWakeupSymbolRxWindow;
    public uint gdWakeupSymbolTxIdle;
    public uint gdWakeupSymbolTxLow;
    public uint pAllowHaltDueToClock;
    public uint pAllowPassiveToActive;
    public uint pChannels;
    public uint pClusterDriftDamping;
    public uint pDecodingCorrection;
    public uint pDelayCompensationA;
    public uint pDelayCompensationB;
    public uint pExternOffsetCorrection;
    public uint pExternRateCorrection;
    public uint pKeySlotUsedForStartup;
    public uint pKeySlotUsedForSync;
    public uint pLatestTx;
    public uint pMacroInitialOffsetA;
    public uint pMacroInitialOffsetB;
    public uint pMaxPayloadLengthDynamic;
    public uint pMicroInitialOffsetA;
    public uint pMicroInitialOffsetB;
    public uint pMicroPerCycle;
    public uint pMicroPerMacroNom;
    public uint pOffsetCorrectionOut;
    public uint pRateCorrectionOut;
    public uint pSamplesPerMicrotick;
    public uint pSingleSlotEnabled;
    public uint pWakeupChannel;
    public uint pWakeupPattern;
    public uint pdAcceptedStartupRange;
    public uint pdListenTimeout;
    public uint pdMaxDrift;
    public uint pdMicrotick;
    public uint gdCASRxLowMax;
    public uint gChannels;
    public uint vExternOffsetControl;
    public uint vExternRateControl;
    public uint pChannelsMTS;
    public uint[] reserved = new uint[16];

    //--------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Returns a collection with name and content of each variable in this class.
    /// </summary>
    /// <returns>Collection of cluster parameters</returns>
    //--------------------------------------------------------------------------------------------------------
    public SortedList GetClusterConfigDescription()
    {
      SortedList paramList = new SortedList();
      FieldInfo[] variables;
      variables = this.GetType().GetFields();
      foreach (FieldInfo var in variables)
      {
        paramList.Add(var.Name, var.GetValue(this).ToString());
      }
      return paramList;
    }

  }
}