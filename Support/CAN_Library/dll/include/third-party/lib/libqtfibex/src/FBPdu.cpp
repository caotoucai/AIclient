#include "FXPduType.h"
#include "FBPdu.h"

namespace ASAM {
namespace FIBEX {

FBPdu::FBPdu()
{
    /* nothing to do here */
}

FBPdu::~FBPdu()
{
    m_signals.clear();
}

QString FBPdu::name() const
{
    return m_fxPdu?m_fxPdu->m_shortName:QString();
}

quint32 FBPdu::byteLength() const
{
    return m_fxPdu?m_fxPdu->m_byteLength:0;
}

FibexTypes::FXPduTypeSt FBPdu::pduType() const
{
    return m_fxPdu?m_fxPdu->m_pduType:FibexTypes::FXPduTypeSt::APPLICATION;
}


} // FIBEX
} // ASAM
