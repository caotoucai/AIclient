#include "../inc/FRChannelType.h"
#include "../inc/FBChannel.h"

namespace ASAM {
namespace FIBEX {

FBChannel::FBChannel()
{
    /* nothing to do here */
}

FBChannel::~FBChannel()
{
    qDeleteAll(m_frameTriggerings);
    m_frameTriggerings.clear();

    qDeleteAll(m_pduTriggerings);
    m_pduTriggerings.clear();

    m_frames.clear();
    m_slotFrames.clear();
    m_ecus.clear();
}

QString FBChannel::name() const
{
    return m_frChannel? m_frChannel->m_shortName : QString();
}

} // FIBEX
} // ASAM
