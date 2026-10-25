#include "FXSignalGroupType.h"
#include "FBSignalGroup.h"

namespace ASAM {
namespace FIBEX {

FBSignalGroup::FBSignalGroup()
{
    /* nothing to do here */
}

FBSignalGroup::~FBSignalGroup()
{
    m_signals.clear();
}

QString FBSignalGroup::name() const
{
    return m_fxSignalGroup?m_fxSignalGroup->m_shortName:QString();
}

} // FIBEX
} // ASAM
