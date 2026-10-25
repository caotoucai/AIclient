#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FibexTypes.h"

class QDomElement;

namespace ASAM {
namespace FIBEX {

/**
 * @brief complexType KEY-SLOT-USAGE
 *
 * Flag indicating whether the Key Slot is used to transmit a startup frame and
 * or a sync frame.
 *
 * (pKeySlotUsedForStartup, pKeySlotUsedForSync)
 *
 */
class ASAM_FIBEX_EXPORT FRKeySlotUsage
{
public:
    FRKeySlotUsage();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    FibexTypes::FRKeySlotUsageSt m_keySlotUsage;
    /** on STARTUP-SYNC or SYNC */
    quint16 m_keySlotId;
};

} // FIBEX
} // ASAM
