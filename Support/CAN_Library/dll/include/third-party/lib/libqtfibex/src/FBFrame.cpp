#include "FXFrameType.h"
#include "FBFrame.h"

namespace ASAM {
namespace FIBEX {

FBFrame::FBFrame()
    : m_slotId(0)
    , m_baseCycle(0)
    , m_cycleRepetition(0)
    , m_cycleCounter(0)
    , m_isTimeTriggered(false)
{
    /* nothing to do here */
}

FBFrame::~FBFrame()
{
    m_pdus.clear();
}

QString FBFrame::name() const
{
    return m_fxFrame?m_fxFrame->m_shortName:QString();
}

quint32 FBFrame::byteLength() const
{
    return m_fxFrame?m_fxFrame->m_byteLength:0;
}

FibexTypes::FXFrameTypeSt FBFrame::frameType() const
{
    return m_fxFrame?m_fxFrame->m_frameType:FibexTypes::FXFrameTypeSt::APPLICATION;
}

} // FIBEX
} // ASAM
