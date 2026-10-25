#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FibexTypes.h"
#include "FXRevisedElementType.h"
#include "FXIdentifier.h"
#include "FXOrderedSignal.h"

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType SIGNAL-GROUP-TYPE
 *
 * Content model for the entity SIGNAL-GROUP
 */
class ASAM_FIBEX_EXPORT FXSignalGroupType : public FXRevisedElementType
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FXSignalGroupType(FXFibex *fibex);
    ~FXSignalGroupType();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @todo element BIT-LENGTH */
    quint32 *m_bitLength = nullptr;
    /** @todo element IDENTIFIER */
    FXIdentifier *m_identifier = nullptr;
    /** @todo element ORDERED-SIGNALS */
    QList<FXOrderedSignal *> m_orderedSignals;
    /** @todo element MANUFACTURER-EXTENSION */
};

} // FIBEX
} // ASAM
