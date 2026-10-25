#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXRevisedElementType.h"
#include "FXPduInstances.h"

class QDomElement;

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType FRAME-TYPE
 *
 * Content model for the entity FRAME.
 */
class ASAM_FIBEX_EXPORT FXFrameTypeCt : public FXRevisedElementType
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FXFrameTypeCt(FXFibex *fibex = nullptr);
    ~FXFrameTypeCt();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    quint32 m_byteLength;
    FibexTypes::FXFrameTypeSt m_frameType;
    FXPduInstances *m_pduInstances = nullptr;
    /** @todo BYTE-LENGTH */
    /** @todo FRAME-TYPE */
    /** @todo PDU-INSTANCES */
    /** @todo MANUFACTURER-EXTENSION */
};

}
}

