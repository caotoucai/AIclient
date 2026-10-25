#include "FXEcuType.h"
#include "FBEcu.h"

namespace ASAM {
namespace FIBEX {

FBEcu::FBEcu()
{
    /* nothing to do here */
}

FBEcu::~FBEcu()
{
    m_inputFrames.clear();
    m_outputFrames.clear();
    m_inputPdus.clear();
}

QString FBEcu::name() const
{
    return m_fxEcu?m_fxEcu->m_shortName:QString();
}

} // FIBEX
} // ASAM
