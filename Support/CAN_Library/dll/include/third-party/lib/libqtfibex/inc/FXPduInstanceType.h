#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXGenericPduInstanceType.h"
#include "FXCommonLayoutDetails.h"

class QDomElement;

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType PDU-INSTANCE-TYPE
 *
 * Content model for the entity PDU-INSTANCE.
 */
class ASAM_FIBEX_EXPORT FXPduInstanceType : public FXGenericPduInstanceType
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FXPduInstanceType(FXFibex *fibex);

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @must group COMMON-LAYOUT-DETAILS */
    FXCommonLayoutDetails m_commLayoutDetails;
    /** element PDU-UPDATE-BIT-POSITION */
    quint32 *m_pduUpdateBitPosition = nullptr;
};

} // FIBEX
} // ASAM
