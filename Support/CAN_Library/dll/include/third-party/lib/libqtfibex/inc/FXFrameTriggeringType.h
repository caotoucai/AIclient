#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXFrameTimings.h"
#include "FXIdentifier.h"

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType FRAME-TRIGGERING-TYPE
 *
 * Content model for the entity FRAME-TRIGGERING.
 */
class ASAM_FIBEX_EXPORT FXFrameTriggeringType
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FXFrameTriggeringType(FXFibex *fibex = nullptr);
    ~FXFrameTriggeringType();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @todo element TIMINGS */
    FXFrameTimings *m_timings = nullptr;
    /** @todo element IDENTIFIER */
    FXIdentifier m_identifier;
    /** @todo element FRAME-REF */
    QString m_frameRef;
    /** @todo element MANUFACTURER-EXTENSION */
    /** @todo attribute ID */
    QString m_id;
};

} // FIBEX
} // ASAM

