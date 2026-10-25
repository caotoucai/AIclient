#include "FibexTypes.h"

namespace ASAM {
namespace FIBEX {

#if 0
const char* const FibexTypes::FXFrameTypeEl_enumString[] = {
    "APPLICATION",
    "BAP",
    "DIAG-REQUEST",
    "DIAG-RESPONSE",
    "DIAG-STATE",
    "NM",
    "OTHER",
    "SERVICE",
    "TPL",
    "XCP_PRE_CONFIGURED",
    "XCP_RUNTIME_CONFIGURED",
};
#endif

FibexTypes::FibexTypes()
{

}

template<>
FibexTypes::EnumParser<FibexTypes::FXFrameTypeSt>::EnumParser()
{
    enumMap["APPLICATION"]              = FibexTypes::FXFrameTypeSt::APPLICATION;
    enumMap["BAP"]                      = FibexTypes::FXFrameTypeSt::BAP;
    enumMap["DIAG-REQUEST"]             = FibexTypes::FXFrameTypeSt::DIAG_REQUEST;
    enumMap["DIAG-RESPONSE"]            = FibexTypes::FXFrameTypeSt::DIAG_RESPONSE;
    enumMap["DIAG-STATE"]               = FibexTypes::FXFrameTypeSt::DIAG_STATE;
    enumMap["NM"]                       = FibexTypes::FXFrameTypeSt::NM;
    enumMap["OTHER"]                    = FibexTypes::FXFrameTypeSt::OTHER;
    enumMap["SERVICE"]                  = FibexTypes::FXFrameTypeSt::SERVICE;
    enumMap["TPL"]                      = FibexTypes::FXFrameTypeSt::TPL;
    enumMap["XCP_PRE_CONFIGURED"]       = FibexTypes::FXFrameTypeSt::XCP_PRE_CONFIGURED;
    enumMap["XCP_RUNTIME_CONFIGURED"]   = FibexTypes::FXFrameTypeSt::XCP_RUNTIME_CONFIGURED;
}

template<>
FibexTypes::EnumParser<FibexTypes::FXPduTypeSt>::EnumParser()
{
    enumMap["APPLICATION"]              = FibexTypes::FXPduTypeSt::APPLICATION;
    enumMap["BAP"]                      = FibexTypes::FXPduTypeSt::BAP;
    enumMap["DIAG-REQUEST"]             = FibexTypes::FXPduTypeSt::DIAG_REQUEST;
    enumMap["DIAG-RESPONSE"]            = FibexTypes::FXPduTypeSt::DIAG_RESPONSE;
    enumMap["DIAG-STATE"]               = FibexTypes::FXPduTypeSt::DIAG_STATE;
    enumMap["NM"]                       = FibexTypes::FXPduTypeSt::NM;
    enumMap["OTHER"]                    = FibexTypes::FXPduTypeSt::OTHER;
    enumMap["SERVICE"]                  = FibexTypes::FXPduTypeSt::SERVICE;
    enumMap["TPL"]                      = FibexTypes::FXPduTypeSt::TPL;
    enumMap["XCP_PRE_CONFIGURED"]       = FibexTypes::FXPduTypeSt::XCP_PRE_CONFIGURED;
    enumMap["XCP_RUNTIME_CONFIGURED"]   = FibexTypes::FXPduTypeSt::XCP_RUNTIME_CONFIGURED;
}

template<>
FibexTypes::EnumParser<FibexTypes::FXTypeTypeSt>::EnumParser()
{
    enumMap["RESPONSE-ERROR"]           = FibexTypes::FXTypeTypeSt::RESPONSE_ERROR;
    enumMap["REQUEST"]                  = FibexTypes::FXTypeTypeSt::REQUEST;
    enumMap["ALIVE-COUNTER"]            = FibexTypes::FXTypeTypeSt::ALIVE_COUNTER;
    enumMap["CHECKSUM"]                 = FibexTypes::FXTypeTypeSt::CHECKSUM;
    enumMap["OTHER"]                    = FibexTypes::FXTypeTypeSt::OTHER;
}

template<>
FibexTypes::EnumParser<FibexTypes::HOCompuCategorySt>::EnumParser()
{
    enumMap["IDENTICAL"]                = FibexTypes::HOCompuCategorySt::IDENTICAL;
    enumMap["LINEAR"]                   = FibexTypes::HOCompuCategorySt::LINEAR;
    enumMap["SCALE_LINEAR"]             = FibexTypes::HOCompuCategorySt::SCALE_LINEAR;
    enumMap["TEXTTABLE"]                = FibexTypes::HOCompuCategorySt::TEXTTABLE;
    enumMap["BITFIELD-TEXTTABLE"]       = FibexTypes::HOCompuCategorySt::BITFIELD_TEXTTABLE;
    enumMap["TAB-NOINTP"]               = FibexTypes::HOCompuCategorySt::TAB_NOINTP;
    enumMap["FORMULA"]                  = FibexTypes::HOCompuCategorySt::FORMULA;
}

template<>
FibexTypes::EnumParser<FibexTypes::HOIntervalTypeAb>::EnumParser()
{
    enumMap["OPEN"]                     = FibexTypes::HOIntervalTypeAb::OPEN;
    enumMap["CLOSED"]                   = FibexTypes::HOIntervalTypeAb::CLOSED;
    enumMap["INFINITE"]                 = FibexTypes::HOIntervalTypeAb::ITV_INFINITE;
}

template<>
FibexTypes::EnumParser<FibexTypes::FXActiveConditionSystemStateType>::EnumParser()
{
    enumMap["CHANNEL_ACTIVE"]           = FibexTypes::FXActiveConditionSystemStateType::CHANNEL_ACTIVE;
    enumMap["CLAMP_15"]                 = FibexTypes::FXActiveConditionSystemStateType::CLAMP_15;
    enumMap["CLAMP_30"]                 = FibexTypes::FXActiveConditionSystemStateType::CLAMP_30;
    enumMap["CLAMP_87"]                 = FibexTypes::FXActiveConditionSystemStateType::CLAMP_87;
    enumMap["CLAMP_RADIO"]              = FibexTypes::FXActiveConditionSystemStateType::CLAMP_RADIO;
    enumMap["NONE"]                     = FibexTypes::FXActiveConditionSystemStateType::NONE;
    enumMap["OTHER"]                    = FibexTypes::FXActiveConditionSystemStateType::OTHER;
}

template<>
FibexTypes::EnumParser<FibexTypes::FXExecuteConditionSignalStateType>::EnumParser()
{
    enumMap["VALUE_CHANGED"]            = FibexTypes::FXExecuteConditionSignalStateType::VALUE_CHANGED;
    enumMap["VALUE_NOT_CHANGED"]        = FibexTypes::FXExecuteConditionSignalStateType::VALUE_NOT_CHANGED;
    enumMap["VALUE_DEFAULT"]            = FibexTypes::FXExecuteConditionSignalStateType::VALUE_DEFAULT;
    enumMap["VALUE_NOT_DEFAULT"]        = FibexTypes::FXExecuteConditionSignalStateType::VALUE_NOT_DEFAULT;
    enumMap["OTHER"]                    = FibexTypes::FXExecuteConditionSignalStateType::OTHER;
}

template<>
FibexTypes::EnumParser<FibexTypes::FXBitCountingPolicy>::EnumParser()
{
    enumMap["MONOTONE"]                 = FibexTypes::FXBitCountingPolicy::MONOTONE;
    enumMap["SAWTOOTH"]                 = FibexTypes::FXBitCountingPolicy::SAWTOOTH;
}

template<>
FibexTypes::EnumParser<FibexTypes::FXMedium>::EnumParser()
{
    enumMap["ELECTRICAL"]               = FibexTypes::FXMedium::ELECTRICAL;
    enumMap["OPTICAL"]                  = FibexTypes::FXMedium::OPTICAL;
    enumMap["OTHER"]                    = FibexTypes::FXMedium::OTHER;
}

template<>
FibexTypes::EnumParser<FibexTypes::HOBaseDataType>::EnumParser()
{
    enumMap["A_UINT8"]                  = FibexTypes::HOBaseDataType::A_UINT8;
    enumMap["A_INT8"]                   = FibexTypes::HOBaseDataType::A_INT8;
    enumMap["A_UINT16"]                 = FibexTypes::HOBaseDataType::A_UINT16;
    enumMap["A_INT16"]                  = FibexTypes::HOBaseDataType::A_INT16;
    enumMap["A_UINT32"]                 = FibexTypes::HOBaseDataType::A_UINT32;
    enumMap["A_INT32"]                  = FibexTypes::HOBaseDataType::A_INT32;
    enumMap["A_UINT64"]                 = FibexTypes::HOBaseDataType::A_UINT64;
    enumMap["A_INT64"]                  = FibexTypes::HOBaseDataType::A_INT64;
    enumMap["A_FLOAT32"]                = FibexTypes::HOBaseDataType::A_FLOAT32;
    enumMap["A_FLOAT64"]                = FibexTypes::HOBaseDataType::A_FLOAT64;
    enumMap["A_ASCIISTRING"]            = FibexTypes::HOBaseDataType::A_ASCIISTRING;
    enumMap["A_UNICODE2STRING"]         = FibexTypes::HOBaseDataType::A_UNICODE2STRING;
    enumMap["A_BYTEFIELD"]              = FibexTypes::HOBaseDataType::A_BYTEFIELD;
    enumMap["A_BITFIELD"]               = FibexTypes::HOBaseDataType::A_BITFIELD;
    enumMap["OTHER"]                    = FibexTypes::HOBaseDataType::OTHER;
}

} // FIBEX
} // ASAM
