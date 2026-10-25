#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXRevisedElementType.h"
#include "FXSignalInstances.h"

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType PDU-TYPE
 *
 * Content model for the entity PDU.
 */
class ASAM_FIBEX_EXPORT FXPduTypeCt : public FXRevisedElementType
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FXPduTypeCt(FXFibex *fibex);
    ~FXPduTypeCt();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @must element fx:BYTE-LENGTH */
    quint32 m_byteLength;
    /** @must element fx:PDU-TYPE */
    FibexTypes::FXPduTypeSt m_pduType;
    /** @opt element fx:SIGNAL-INSTANCES */
    FXSignalInstances *m_sigInstances = nullptr;

    /** @todo element MULTIPLEXER */
    /** @todo element MANUFACTURER-EXTENSION */
};

} // FIBEX
} // ASAM
