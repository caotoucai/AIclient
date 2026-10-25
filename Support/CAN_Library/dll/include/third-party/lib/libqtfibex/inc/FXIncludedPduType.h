#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXIncludedSignalType.h"

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType INCLUDED-PDU-TYPE
 *
 * SIGNALs within a referenced PDU
 */
class ASAM_FIBEX_EXPORT FXIncludedPduType
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FXIncludedPduType(FXFibex *fibex = nullptr);
    ~FXIncludedPduType();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** element PDU-TRIGGERING-REF */
    QString m_pduTriggeringRef;
    /** element INCLUDED-SIGNALS */
    QList<FXIncludedSignalType *> m_includedSignals;
    /** element COMPLETE-PDU */
    bool m_completePdu;
    /** @todo attribute ID */
};

} // FIBEX
} // ASAM
