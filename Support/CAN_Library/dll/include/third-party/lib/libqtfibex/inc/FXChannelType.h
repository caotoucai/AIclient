#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXRevisedElementType.h"
#include "FXPduTriggeringType.h"
#include "FXFrameTriggeringType.h"

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType CHANNEL-TYPE
 *
 * Content model for the entity CHANNEL with platform specific extendability.
 */
class ASAM_FIBEX_EXPORT FXChannelType : public FXRevisedElementType
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FXChannelType(FXFibex *fibex);
    ~FXChannelType();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @todo element PDU-TRIGGERINGS */
    QHash<QString, FXPduTriggeringType*> m_pduTriggerings;
    /** @todo element FRAME-TRIGGERINGS */
    QHash<QString, FXFrameTriggeringType*> m_frameTriggerings;
    /** @todo element MANUFACTURER-EXTENSION */
};

} // FIBEX
} // ASAM
