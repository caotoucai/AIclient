#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXPduTimings.h"

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType PDU-TRIGGERING-TYPE
 *
 * Content model for the entity PDU-TRIGGERING.
 */
class ASAM_FIBEX_EXPORT FXPduTriggeringType
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FXPduTriggeringType(FXFibex *fibex = nullptr);
    ~FXPduTriggeringType();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @opt element TIMINGS */
    FXPduTimings *m_timings = nullptr;
    /** @must element fx:PDU-REF */
    QString m_pduRef;
    /** @todo element MANUFACTURER-EXTENSION */
    /** @must attribute ID */
    QString m_id;
};

} // FIBEX
} // ASAM
