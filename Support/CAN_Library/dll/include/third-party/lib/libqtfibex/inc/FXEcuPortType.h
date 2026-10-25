#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXIncludedPduType.h"

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType ECU-PORT-TYPE
 *
 * Content model for the entity PORT in the ECU related peculiarity.
 */
class ASAM_FIBEX_EXPORT FXEcuPortType
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FXEcuPortType(FXFibex *fibex = nullptr);
    ~FXEcuPortType();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** attribute ID */
    QString m_id;

    /** element ho:DESC */
    /** element FRAME-TRIGGERING-REF */
    QString m_frameTriggeringRef;
    /** element INCLUDED-PDUS */
    QList<FXIncludedPduType *> m_includedPdus;
    /** element COMPLETE-FRAME */
    bool m_completeFrame;
    /** @todo element MANUFACTURER-EXTENSION */

};

} // FIBEX
} // ASAM
